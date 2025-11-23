using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proj1.Data;
using proj1.Models;
using proj1.Services;
using proj1.Constants;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace proj1.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PasswordService _passwordService;

        public AuthController(AppDbContext context, PasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole(Roles.Admin))
                {
                    return RedirectToAction("Index", "Admin");
                }
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                TempData["Message"] = "Kullanıcı adı ve şifre gereklidir.";
                TempData["Type"] = "error";
                return View();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                TempData["Message"] = "Kullanıcı adı veya şifre hatalı.";
                TempData["Type"] = "error";
                return View();
            }

            if (!_passwordService.VerifyPassword(user.PasswordHash, password))
            {
                TempData["Message"] = "Kullanıcı adı veya şifre hatalı.";
                TempData["Type"] = "error";
                return View();
            }

            // Check if rehash is needed (upgrade from legacy to PBKDF2)
            if (_passwordService.IsRehashNeeded(user.PasswordHash))
            {
                user.PasswordHash = _passwordService.HashPassword(password);
                await _context.SaveChangesAsync();
            }

            // Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            TempData["Message"] = $"Hoş geldiniz, {user.Username}!";
            TempData["Type"] = "success";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            if (user.Role == Roles.Admin)
            {
                return RedirectToAction("Index", "Admin");
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole(Roles.Admin))
                {
                    return RedirectToAction("Index", "Admin");
                }
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string username, string email, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["Message"] = "Alanlar boş bırakılamaz.";
                TempData["Type"] = "error";
                return View();
            }
            if (password != confirmPassword)
            {
                TempData["Message"] = "Şifreler uyuşmuyor.";
                TempData["Type"] = "error";
                return View();
            }
            if (password.Length < 8)
            {
                TempData["Message"] = "Şifre en az 8 karakter olmalıdır.";
                TempData["Type"] = "error";
                return View();
            }
            if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                TempData["Message"] = "Kullanıcı adı zaten alınmış.";
                TempData["Type"] = "error";
                return View();
            }
            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                TempData["Message"] = "E-posta zaten kayıtlı.";
                TempData["Type"] = "error";
                return View();
            }

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = _passwordService.HashPassword(password),
                Role = Roles.User,
                CreatedAt = DateTime.Now
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Kayıt başarılı. Oturum açıldı.";
            TempData["Type"] = "success";

            // Otomatik giriş
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmNewPassword)
        {
            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                TempData["Message"] = "Alanlar boş bırakılamaz.";
                TempData["Type"] = "error";
                return View();
            }
            if (newPassword != confirmNewPassword)
            {
                TempData["Message"] = "Yeni şifreler uyuşmuyor.";
                TempData["Type"] = "error";
                return View();
            }
            if (newPassword.Length < 8)
            {
                TempData["Message"] = "Şifre en az 8 karakter olmalı.";
                TempData["Type"] = "error";
                return View();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                TempData["Message"] = "Oturum bulunamadı.";
                TempData["Type"] = "error";
                return RedirectToAction("Login");
            }
            var uid = int.Parse(userId);
            var user = await _context.Users.FindAsync(uid);
            if (user == null)
            {
                TempData["Message"] = "Kullanıcı bulunamadı.";
                TempData["Type"] = "error";
                return RedirectToAction("Login");
            }

            // Mevcut şifre doğrula
            if (!_passwordService.VerifyPassword(user.PasswordHash, currentPassword))
            {
                TempData["Message"] = "Mevcut şifre yanlış.";
                TempData["Type"] = "error";
                return View();
            }

            // Yeni şifreyi PBKDF2 ile kaydet
            user.PasswordHash = _passwordService.HashPassword(newPassword);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Şifre başarıyla güncellendi.";
            TempData["Type"] = "success";
            
            if (user.Role == Roles.Admin)
            {
                return RedirectToAction("Index", "Admin");
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Auth", new { returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            if (remoteError != null)
            {
                TempData["Message"] = $"Harici sağlayıcı hatası: {remoteError}";
                TempData["Type"] = "error";
                return RedirectToAction("Login");
            }

            // Google'dan gelen bilgileri al
            var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            // Eğer cookie şeması ile alamazsak, geçici olarak Google şemasını deneyelim
            if (!authenticateResult.Succeeded)
            {
                // Bazen Google auth sonucu doğrudan Google şemasında kalabilir
                // Ancak AddGoogle varsayılan olarak SignInScheme kullanır.
                // Hata durumunda buraya düşüyorsa, muhtemelen OnRemoteFailure tetiklenmiştir.
                // Ancak yine de güvenli tarafta kalmak için kontrol edelim.
                TempData["Message"] = "Google doğrulaması başarısız oldu.";
                TempData["Type"] = "error";
                return RedirectToAction("Login");
            }

            var email = authenticateResult.Principal.FindFirstValue(ClaimTypes.Email);
            if (email == null)
            {
                TempData["Message"] = "Google'dan e-posta bilgisi alınamadı.";
                TempData["Type"] = "error";
                return RedirectToAction("Login");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                // Otomatik kayıt
                user = new User
                {
                    Username = email.Split('@')[0],
                    Email = email,
                    PasswordHash = _passwordService.HashPassword(Guid.NewGuid().ToString()), // Rastgele şifre
                    Role = Roles.User,
                    CreatedAt = DateTime.Now
                };

                // Kullanıcı adı çakışması kontrolü
                if (await _context.Users.AnyAsync(u => u.Username == user.Username))
                {
                    user.Username += new Random().Next(1000, 9999);
                }

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            // Kendi claimlerimizle oturum aç
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
            };

            // Önce Google oturumunu kapat (çünkü aynı cookie şemasını kullanıyoruz)
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Yeni oturumu aç
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            TempData["Message"] = $"Hoş geldiniz, {user.Username}!";
            TempData["Type"] = "success";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            if (user.Role == Roles.Admin)
            {
                return RedirectToAction("Index", "Admin");
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Message"] = "Başarıyla çıkış yaptınız.";
            TempData["Type"] = "success";
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}


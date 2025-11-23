using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proj1.Data;
using proj1.Models;
using Microsoft.AspNetCore.Authorization;
using proj1.Constants;

namespace proj1.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories.ToListAsync());
        }

        public IActionResult Add()
        {
            return View();
        }

        [AcceptVerbs("GET", "POST")]
        public IActionResult VerifyName(string name, int? id)
        {
            if (_context.Categories.Any(c => c.Name == name && (id == null || c.Id != id)))
            {
                return Json($"'{name}' zaten kullanımda.");
            }

            return Json(true);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("Name,IsActive")] Category category)
        {
            if (_context.Categories.Any(c => c.Name == category.Name))
            {
                ModelState.AddModelError("Name", "Bu kategori adı zaten mevcut.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Kategori başarıyla eklendi.";
                TempData["Type"] = "success"; // Yeşil
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, [Bind("Id,Name,IsActive")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (_context.Categories.Any(c => c.Name == category.Name && c.Id != id))
            {
                ModelState.AddModelError("Name", "Bu kategori adı zaten mevcut.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Kategori başarıyla güncellendi.";
                    TempData["Type"] = "warning"; // Sarı/Turuncu (Düzenleme için)
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Kategori başarıyla silindi.";
                TempData["Type"] = "error"; // Kırmızı (Silme için)
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            category.IsActive = !category.IsActive;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Durum güncellendi.", isActive = category.IsActive });
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}

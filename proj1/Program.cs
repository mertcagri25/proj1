using Microsoft.EntityFrameworkCore;
using proj1.Data;
using proj1.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text.Encodings.Web; // Added for UrlEncoder

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Services
builder.Services.AddScoped<PasswordService>();

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.SlidingExpiration = true;
        options.Cookie.Name = "NewsPortalAuth";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "YOUR_CLIENT_ID";
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "YOUR_CLIENT_SECRET";
        
        // Bağlantı hatalarını yakalayıp login sayfasına yönlendir
        options.Events.OnRemoteFailure = ctx =>
        {
            var errorMessage = UrlEncoder.Default.Encode(ctx.Failure?.Message ?? "Bilinmeyen bir hata oluştu.");
            ctx.Response.Redirect($"/Auth/Login?remoteError={errorMessage}");
            ctx.HandleResponse();
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

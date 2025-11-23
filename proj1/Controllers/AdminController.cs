using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proj1.Data;
using Microsoft.AspNetCore.Authorization;
using proj1.Constants;

namespace proj1.Controllers
{
    [Authorize(Roles = Roles.Admin)] // Sadece Admin rolü erişebilir
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // İstatistikler
            ViewBag.TotalCategories = await _context.Categories.CountAsync();
            ViewBag.ActiveCategories = await _context.Categories.Where(c => c.IsActive).CountAsync();
            ViewBag.TotalNews = await _context.News.CountAsync();
            ViewBag.PublishedNews = await _context.News.Where(n => n.IsPublished).CountAsync();
            ViewBag.TotalUsers = await _context.Users.CountAsync();

            // Son eklenen haberler
            ViewBag.RecentNews = await _context.News
                .Include(n => n.Category)
                .OrderByDescending(n => n.PublishDate)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }
}

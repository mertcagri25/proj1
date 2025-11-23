using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proj1.Data;
using proj1.Models;

namespace proj1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel
            {
                SliderNews = await _context.News
                    .Include(n => n.Category)
                    .Where(n => n.IsPublished)
                    .OrderByDescending(n => n.PublishDate)
                    .Take(5)
                    .ToListAsync(),

                LatestNews = await _context.News
                    .Include(n => n.Category)
                    .Where(n => n.IsPublished)
                    .OrderByDescending(n => n.PublishDate)
                    .Skip(5)
                    .Take(10)
                    .ToListAsync(),

                Categories = await _context.Categories
                    .Where(c => c.IsActive)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

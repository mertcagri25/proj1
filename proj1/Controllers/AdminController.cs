using Microsoft.AspNetCore.Mvc;

namespace proj1.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

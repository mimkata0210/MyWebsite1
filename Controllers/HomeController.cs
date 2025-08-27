using Microsoft.AspNetCore.Mvc;
using MyWebsite1.Models;
using Microsoft.EntityFrameworkCore;
using MyWebsite1.Data;

namespace MyWebsite1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewData["CurrentPage"] = "Index";
            return View();
        }

        public IActionResult Privacy()
        {
            ViewData["CurrentPage"] = "Privacy";
            return View();
        }

        public IActionResult About()
        {
            ViewData["CurrentPage"] = "About";
            return View();
        }

        public IActionResult Contacts()
        {
            ViewData["CurrentPage"] = "Contacts";
            return View();
        }
    }
}

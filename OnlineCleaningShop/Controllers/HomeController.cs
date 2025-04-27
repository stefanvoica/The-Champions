using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineCleaningShop.Data;
using OnlineCleaningShop.Models;
using System.Diagnostics;

namespace OnlineCleaningShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly ApplicationDbContext _db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public HomeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<HomeController> logger

            )
        {
            _db = context;

            _userManager = userManager;

            _roleManager = roleManager;

            _logger = logger;

        }

        public IActionResult Index()
        {
            if (User.IsInRole("Admin"))
            {
                var categories = _db.Categories.OrderBy(c => c.CategoryName).ToList();
                ViewBag.Categories = categories;
            }
            else
            {
                ViewBag.Categories = null;
            }

            // de selectat doar produsele aprobate de admin; in curand!
            var products = from product in _db.Products
                           select product;
            if (products.Count() == 0)
            {
                //TempData["message"] = "No products in the database!";
                return View();
            }
            ViewBag.FirstProduct = products.First();
            ViewBag.Products = products.OrderBy(o => o.Name).Skip(1).Take(2);
            return View();
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

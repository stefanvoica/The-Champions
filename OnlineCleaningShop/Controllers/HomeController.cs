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

        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public HomeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<HomeController> logger

            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;

            _logger = logger;

        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Products");
            }

            var products = from product in db.Products
                           select product;
            if (products.Count() == 0)
            {
                TempData["message"] = "No products in the database!";
                return View();
            }
            ViewBag.FirstProduct = products.First();
            ViewBag.Products = products.OrderBy(o => o.Title).Skip(1).Take(2);
            ViewBag.Message = TempData["message"];
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

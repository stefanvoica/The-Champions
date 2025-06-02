using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineCleaningShop.Models;
using OnlineCleaningShop.Data;
using System.Diagnostics;
using OnlineCleaningShop.Data;
using OnlineCleaningShop.Models;

namespace OnlineCleaningShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _db = db;
            _userManager = userManager;
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

            var acceptedProducts = _db.Products
    .Join(
        _db.ProductRequests,
        product => product.Id,
        requestProduct => requestProduct.ProductId,
        (product, requestProduct) => new { Product = product, RequestProduct = requestProduct }
    )
    .Where(pr => pr.RequestProduct.Status == RequestStatus.Approved && pr.Product.Stock > 0)
    .OrderBy(r => Guid.NewGuid())
    .Take(4)
    .Select(pr => pr.Product)
    .ToList();


            ViewBag.Products = acceptedProducts;

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
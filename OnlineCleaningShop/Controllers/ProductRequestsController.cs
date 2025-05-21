using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineCleaningShop.Data;
using OnlineCleaningShop.Models;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineCleaningShop.Controllers
{
    [Authorize]
    public class ProductRequestsController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductRequestsController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        // Adminul vizualizează cererile
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var requests = await _dbContext.ProductRequests
                .Include(pr => pr.Product)
                .Include(pr => pr.Product.Category) // Dacă vrei să afișezi și categoria produsului
                .ToListAsync();

            return View(requests);
        }

        // Colaborator trimite o cerere pentru un produs
        [Authorize(Roles = "Colaborator")]
        public IActionResult Create(int productId)
        {
            var request = new ProductRequest
            {
                ProductId = productId,
                UserId = _userManager.GetUserId(User)
            };

            return View(request);
        }

        [HttpPost]
        [Authorize(Roles = "Colaborator")]
        public async Task<IActionResult> Create(ProductRequest request)
        {
            if (ModelState.IsValid)
            {
                _dbContext.ProductRequests.Add(request);
                await _dbContext.SaveChangesAsync();
                TempData["message"] = "Cererea a fost trimisă cu succes!";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index", "Products");
            }

            TempData["message"] = "Eroare la trimiterea cererii.";
            TempData["messageType"] = "alert-danger";
            return View(request);
        }

        // Admin aprobă sau respinge o cerere
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            // Găsim cererea
            var request = await _dbContext.ProductRequests
                .Include(pr => pr.Product)
                .FirstOrDefaultAsync(pr => pr.Id == id);

            if (request == null)
            {
                TempData["message"] = "Cererea nu a fost găsită.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            // Găsim produsul asociat cererii
            var product = await _dbContext.Products.FindAsync(request.ProductId);

            if (product != null)
            {
                // Aplicăm modificările asupra produsului
                product.Name = request.Product.Name;
                product.Description = request.Product.Description;
                product.Price = request.Product.Price;
                product.Stock = request.Product.Stock;
                product.CategoryId = request.Product.CategoryId;
                product.Image = request.Product.Image;

                // Marcăm cererea ca aprobată (opțional) sau o ștergem
                _dbContext.ProductRequests.Remove(request);

                // Salvăm modificările
                await _dbContext.SaveChangesAsync();

                TempData["message"] = "Cererea a fost aprobată și modificările au fost aplicate.";
                TempData["messageType"] = "alert-success";
            }
            else
            {
                TempData["message"] = "Produsul nu a fost găsit.";
                TempData["messageType"] = "alert-danger";
            }

            return RedirectToAction("Index");
        }



        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(int id)
        {
            var request = await _dbContext.ProductRequests.FindAsync(id);

            if (request == null)
            {
                TempData["message"] = "Cererea nu a fost găsită.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            _dbContext.ProductRequests.Remove(request);
            await _dbContext.SaveChangesAsync();

            TempData["message"] = "Cererea a fost respinsă.";
            TempData["messageType"] = "alert-success";
            return RedirectToAction("Index");
        }
    }
}

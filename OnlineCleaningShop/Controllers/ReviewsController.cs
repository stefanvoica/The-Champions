using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineCleaningShop.Data;
using OnlineCleaningShop.Models;

namespace OnlineCleaningShop.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public ReviewsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Adaugarea unui review asociat unui produs
        [HttpPost]
        [Authorize(Roles = "User,Colaborator,Admin")]
        public IActionResult New(Review review)
        {
            review.Date = DateTime.Now;
            review.UserId = _userManager.GetUserId(User);

            try
            {
                db.Reviews.Add(review);
                db.SaveChanges();

                UpdateProductScore(review.ProductId);

                return Redirect("/Products/Show/" + review.ProductId);
            }
            catch (Exception)
            {
                return Redirect("/Products/Show/" + review.ProductId);
            }
        }

        // Stergerea unui review
        [HttpPost]
        [Authorize(Roles = "User,Colaborator,Admin")]
        public IActionResult Delete(int id)
        {
            Review review = db.Reviews.Find(id);

            if (review.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Reviews.Remove(review);
                db.SaveChanges();

                UpdateProductScore(review.ProductId);

                return Redirect("/Products/Show/" + review.ProductId);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti review-ul";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }

        }

        // Editarea unui review
        [Authorize(Roles = "User,Colaborator,Admin")]
        public IActionResult Edit(int id)
        {
            Review review = db.Reviews.Find(id);

            if (review.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                return View(review);
            else
            {
                TempData["message"] = "Nu aveti dreptul sa editati review-ul";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }
            return View();
        }


        [HttpPost]
        [Authorize(Roles = "User,Colaborator,Admin")]
        public IActionResult Edit(int id, Review requestReview)
        {
            Review review = db.Reviews.Find(id);

            if (review.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                if (ModelState.IsValid)
                {
                    review.Text = requestReview.Text;
                    review.Rating = requestReview.Rating;
                    db.SaveChanges();

                    UpdateProductScore(review.ProductId);

                    return Redirect("/Products/Show/" + review.ProductId);
                }
                else return View(requestReview);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa editati review-ul";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }
        }


        private void UpdateProductScore(int productId)
        {
            var product = db.Products.Include(p => p.Reviews).FirstOrDefault(p => p.Id == productId);
            if (product != null && product.Reviews.Any())
            {
                product.Score = product.Reviews.Average(r => r.Rating);
            }
            else
            {
                product.Score = null;
            }
            db.SaveChanges();
        }

    }

}
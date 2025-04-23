using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineCleaningShop.Data;
using OnlineCleaningShop.Models;

namespace OnlineCleaningShop.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public CommentsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        // Stergerea unui comentariu asociat unui articol din baza de date
        // Se poate sterge comentariul doar de catre admini sau de catre utilizatorii user sau colaborator, doar daca acel comentariu a fost postat de catre acestia
        [HttpPost]
        [Authorize(Roles = "User,Colaborator,Admin")]
        public IActionResult Delete(int id)
        {
            Comment comm = db.Comments.Find(id);
            if (comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Comments.Remove(comm);
                db.SaveChanges();
                return Redirect("/Products/Show/" + comm.ProductId);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti comentariul";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }
        }

        // In acest moment vom implementa editarea intr-o pagina View separata
        // Se editeaza un comentariu existent
        // Editarea unui comentariu asociat unui produs
        // [HttpGet] se executa implicit
        // Se poate edita un comentariu doar de catre utilizatorul care a postat comentariul respectiv
        // Adminii pot edita orice comentariu, chiar daca nu a fost postat de ei

        [Authorize(Roles = "User,Colaborator,Admin")]
        public IActionResult Edit(int id)
        {
            Comment comm = db.Comments.Find(id);

            if (comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(comm);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa editati comentariul";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Colaborator,Admin")]
        public IActionResult Edit(int id, Comment requestComment)
        {
            Comment comm = db.Comments.Find(id);
            if (comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                if (ModelState.IsValid)
                {
                    comm.Content = requestComment.Content;
                    comm.Rating = requestComment.Rating;
                    db.SaveChanges();
                    return Redirect("/Products/Show/" + comm.ProductId);
                }
                else
                {
                    return View(requestComment);
                }
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa editati comentariul";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User,Colaborator,Admin")]
        public IActionResult Add(ProductCommentViewModel vm)
        {
            var comment = vm.CommentContent;
            var productId = vm.ProductId;
            var rating = vm.Rating;

            var userId = _userManager.GetUserId(User);

            Comment comm = new Comment
            {
                Content = comment,
                ProductId = productId,
                UserId = userId,
                Date = DateTime.Now,
                Rating = rating
            };
            db.Comments.Add(comm);
            db.SaveChanges();
            return Redirect("/Products/Show/" + productId);

        }
    }
}

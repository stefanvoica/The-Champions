using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineCleaningShop.Data;
using OnlineCleaningShop.Models;

namespace OnlineCleaningShop.Controllers
{
    [Authorize]
    public class BookmarksController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public BookmarksController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;
        }


        // Toti utilizatorii pot vedea Bookmark-urile existente in platforma
        // Fiecare utilizator vede bookmark-urile pe care le-a creat
        // Userii cu rolul de Admin pot sa vizualizeze toate bookmark-urile existente
        // HttpGet - implicit
        [Authorize(Roles = "User,Colaborator,Admin")]
        public IActionResult Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            SetAccessRights();

            if (User.IsInRole("User") || User.IsInRole("Colaborator"))
            {
                var bookmarks = from bookmark in db.Bookmarks.Include("User")
                               .Where(b => b.UserId == _userManager.GetUserId(User))
                                select bookmark;

                ViewBag.Bookmarks = bookmarks;

                return View();
            }
            else
            if (User.IsInRole("Admin"))
            {
                var bookmarks = from bookmark in db.Bookmarks.Include("User")
                                select bookmark;

                ViewBag.Bookmarks = bookmarks;

                return View();
            }

            else
            {
                TempData["message"] = "Nu aveti drepturi asupra colectiei";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }

        }

        // Afisarea tuturor produselor pe care utilizatorul le-a salvat in 
        // bookmark-ul sau 
        [Authorize(Roles = "User,Colaborator,Admin")]
        public IActionResult Show(int id)
        {
            SetAccessRights();

            if (User.IsInRole("User") || User.IsInRole("Colaborator"))
            {
                var bookmarks = db.Bookmarks
                                  .Include("ProductBookmarks.Product.Category")
                                  .Include("ProductBookmarks.Product.User")
                                  .Include("User")
                                  .Where(b => b.Id == id)
                                  .Where(b => b.UserId == _userManager.GetUserId(User))
                                  .FirstOrDefault();

                if (bookmarks == null)
                {
                    TempData["message"] = "Resursa cautata nu poate fi gasita";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index", "Products");
                }

                return View(bookmarks);
            }

            else
            if (User.IsInRole("Admin"))
            {
                var bookmarks = db.Bookmarks
                                  .Include("ProductBookmarks.Product.Category")
                                  .Include("ProductBookmarks.Product.User")
                                  .Include("User")
                                  .Where(b => b.Id == id)
                                  .FirstOrDefault();


                if (bookmarks == null)
                {
                    TempData["message"] = "Resursa cautata nu poate fi gasita";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index", "Products");
                }


                return View(bookmarks);
            }

            else
            {
                TempData["message"] = "Nu aveti drepturi";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }
        }

        // Randarea formularului in care se completeaza datele unui bookmark
        // [HttpGet] - se executa implicit
        [Authorize(Roles = "User,Colaborator,Admin")]
        public IActionResult New()
        {
            return View();
        }

        // Adaugarea bookmark-ului in baza de date
        [HttpPost]
        [Authorize(Roles = "User,Colaborator,Admin")]
        public ActionResult New(Bookmark bm)
        {
            bm.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Bookmarks.Add(bm);
                db.SaveChanges();
                TempData["message"] = "Colectia a fost adaugata";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }

            else
            {
                return View(bm);
            }
        }


        // Conditiile de afisare a butoanelor de editare si stergere
        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            if (User.IsInRole("Colaborator") || User.IsInRole("User"))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.EsteAdmin = User.IsInRole("Admin");

            ViewBag.UserCurent = _userManager.GetUserId(User);
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineCleaningShop.Data;
using OnlineCleaningShop.Models;
using static NuGet.Packaging.PackagingConstants;

namespace OnlineCleaningShop.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public OrdersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;
        }

        // Toti utilizatorii pot vedea cosurile existente in platforma
        // Fiecare utilizator vede cosurile pe care le-a creat
        // Userii cu rolul de Admin pot sa vizualizeze toate cosurile existente
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
                var orders = from order in db.Orders.Include("User")
                               .Where(b => b.UserId == _userManager.GetUserId(User))
                             select order;

                ViewBag.Orders = orders;

                return View();
            }
            else
            if (User.IsInRole("Admin"))
            {
                var orders = from order in db.Orders.Include("User")
                             select order;

                ViewBag.Orders = orders;

                return View();
            }

            else
            {
                TempData["message"] = "Nu aveti drepturi asupra cosului de cumparaturi";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }

        }

        // Afisarea tuturor produselor pe care utilizatorul le-a salvat
        [Authorize(Roles = "User,Colaborator,Admin")]
        public IActionResult Show(int id)
        {
            SetAccessRights();

            var orders = db.Orders
                              .Include("OrderDetails.Product.Category")
                              .Include("OrderDetails.Product.User")
                              .Include("User")
                              .FirstOrDefault(b => b.Id == id);

            if (orders == null ||
                (User.IsInRole("User") || User.IsInRole("Colaborator")) && orders.UserId != _userManager.GetUserId(User))
            {
                TempData["message"] = "Resursa cautata nu poate fi gasita";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }

            var subtotal = orders.OrderDetails.Sum(od => od.Product.Price * od.Quantity);
            decimal deliveryFee = 0;

            if (orders.DeliveryMethod == DeliveryMethod.Courier)
                deliveryFee = 15;
            else if (orders.DeliveryMethod == DeliveryMethod.Easybox)
                deliveryFee = 8;

            decimal total = (decimal)subtotal + (decimal)deliveryFee;
            ViewBag.TotalInitial = subtotal;
            ViewBag.DeliveryFee = deliveryFee;
            ViewBag.Total = total;

            var cod = Request.Query["promoCode"].ToString().Trim().ToUpper();
            var codPromo = db.CoduriPromotionale.FirstOrDefault(c => c.Nume.ToUpper() == cod);

            if (!string.IsNullOrEmpty(cod))
            {
                if (codPromo != null)
                {
                    decimal reducere = (decimal)codPromo.ProcentReducere;
                    ViewBag.ReducereProcent = reducere * 100;
                    ViewBag.CodAplicat = cod;

                    var totalCuReducere = (decimal) subtotal * ((decimal) (1 - reducere)) + deliveryFee;
                    ViewBag.Total = totalCuReducere;
                }
                else
                {
                    TempData["message"] = "Codul promoțional nu există.";
                    TempData["messageType"] = "alert-danger";
                }
            }

            return View(orders);
        }


        // Randarea formularului in care se completeaza datele unui bookmark
        // [HttpGet] - se executa implicit
        [Authorize(Roles = "User,Colaborator,Admin")]
        public IActionResult New()
        {
            return View();
        }

        // Adaugarea cosului in baza de date
        [HttpPost]
        [Authorize(Roles = "User,Colaborator,Admin")]
        public ActionResult New(Order cos)
        {
            cos.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Orders.Add(cos);
                db.SaveChanges();

                TempData["message"] = "Coșul a fost adăugat!";
                TempData["messageType"] = "alert-success";

                return RedirectToAction("Index", "Products");
            }
            else
            {
                // Dacă modelul nu este valid, se reafișează formularul cu datele introduse
                TempData["message"] = "Datele introduse nu sunt valide. Verificați formularul.";
                TempData["messageType"] = "alert-danger";
                return View(cos);
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
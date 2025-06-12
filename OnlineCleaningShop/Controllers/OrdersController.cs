using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineCleaningShop.Data;
using OnlineCleaningShop.Models;
using static NuGet.Packaging.PackagingConstants;
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Colors;

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

            if (subtotal < 300)
            {
                TempData["message"] = "Mai adauga produse in valoare de " + (300 - subtotal) + " lei pentru a beneficia de livrare gratuita!";
                TempData["messageType"] = "alert-info";
                if (orders.DeliveryMethod == DeliveryMethod.Courier)
                    deliveryFee = 15;
                else if (orders.DeliveryMethod == DeliveryMethod.Easybox)
                    deliveryFee = 8;
            }
            else
            {
                TempData["message"] = "Livrare gratuita!";
                TempData["messageType"] = "alert-success";
            }

            decimal total = (decimal)subtotal + (decimal)deliveryFee;
            ViewBag.TotalInitial = subtotal;
            ViewBag.DeliveryFee = deliveryFee;
            ViewBag.Total = total;

            orders.TotalInitial = subtotal;
            orders.DeliveryFee = deliveryFee;
            orders.Total = total;

            var cod = Request.Query["promoCode"].ToString().Trim().ToUpper();
            var codPromo = db.CoduriPromotionale.FirstOrDefault(c => c.Nume.ToUpper() == cod);

            if (!string.IsNullOrEmpty(cod))
            {
                if (codPromo != null)
                {
                    decimal reducere = (decimal)codPromo.ProcentReducere;
                    ViewBag.ReducereProcent = reducere * 100;
                    ViewBag.CodAplicat = cod;

                    var totalCuReducere = (decimal)subtotal * ((decimal)(1 - reducere)); //+ deliveryFee;

                    if (totalCuReducere < 300)
                    {
                        TempData["message"] = "Mai adauga produse in valoare de " + (300 - totalCuReducere) + " lei pentru a beneficia de livrare gratuita!";
                        TempData["messageType"] = "alert-info";
                        if (orders.DeliveryMethod == DeliveryMethod.Courier)
                            deliveryFee = 15;
                        else if (orders.DeliveryMethod == DeliveryMethod.Easybox)
                            deliveryFee = 8;
                    }
                    else
                    {
                        TempData["message"] = "Livrare gratuita!";
                        TempData["messageType"] = "alert-success";
                        deliveryFee = 0;
                    }
                    ViewBag.TotalCuReducere = totalCuReducere;
                    ViewBag.Total = totalCuReducere + deliveryFee;

                    orders.TotalWithDiscount = totalCuReducere;
                    orders.PromoCode = cod;
                    orders.DeliveryFee = deliveryFee;
                    orders.Total = totalCuReducere + deliveryFee;

                }
                else
                {
                    TempData["message"] = "Codul promoțional nu există.";
                    TempData["messageType"] = "alert-danger";
                }
            }

            db.Orders.Update(orders);
            db.SaveChanges();
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

        public IActionResult DownloadInvoice(int id)
        {
            var order = db.Orders.Include(o => o.User)
                                 .Include(o => o.OrderDetails)
                                 .ThenInclude(od => od.Product)
                                 .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Generate PDF
            using var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // Add content to the PDF
            document.Add(new Paragraph($"Factura pentru comanda #{order.Id}").SetFontSize(20).SimulateBold());
            document.Add(new Paragraph($"Data: {DateTime.Now.Date:dd/MM/yyyy}"));
            document.Add(new Paragraph($"Client: {order.User.UserName}"));
            document.Add(new Paragraph(" "));

            var table = new Table(4).UseAllAvailableWidth();
            table.AddHeaderCell("Produs").SimulateBold();
            table.AddHeaderCell("Pret unitar").SimulateBold();
            table.AddHeaderCell("Cantitate").SimulateBold();
            table.AddHeaderCell("Total").SimulateBold();

            foreach (var detail in order.OrderDetails)
            {
                table.AddCell(detail.Product.Name);
                table.AddCell($"{detail.Product.Price:0.00} lei");
                table.AddCell(detail.Quantity.ToString());
                table.AddCell($"{detail.Product.Price * detail.Quantity:0.00} lei");
            }

            document.Add(table);

            document.Add(new Paragraph(" ").SetMarginTop(10));
            document.Add(new Paragraph($"Subtotal: {order.TotalInitial:0.00} lei").SimulateBold().SetFontSize(14));

            if (!string.IsNullOrEmpty(order.PromoCode))
            {
                document.Add(new Paragraph($"Cod promotional: {order.PromoCode}").SetFontSize(14));
                document.Add(new Paragraph($"Reducere aplicata: {order.TotalInitial - (double)order.TotalWithDiscount:0.00} lei").SetFontSize(14));
                document.Add(new Paragraph($"Totalul cu reducere: {order.TotalWithDiscount:0.00} lei").SetFontSize(14));
            }
            if (order.DeliveryFee > 0)
            {
                document.Add(new Paragraph($"Taxa de livrare: {order.DeliveryFee:0.00} lei").SetFontSize(14));
            }
            else
            {
                document.Add(new Paragraph("Livrare gratuita!").SetFontSize(14).SetFontColor(ColorConstants.GREEN));
            }
            document.Add(new Paragraph($"Total de plata: {order.Total:0.00} lei").SetFontSize(16).SimulateBold());
            document.Close();

            return File(stream.ToArray(), "application/pdf", $"Invoice_Order_{id}.pdf");
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
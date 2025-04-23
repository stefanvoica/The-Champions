using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineCleaningShop.Data;
using OnlineCleaningShop.Models;
using static OnlineCleaningShop.Models.ProductBookmarks;

namespace OnlineCleaningShop.Controllers
{
    public class ProductsController : Controller
    {
        // PASUL 10: useri si roluri 

        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public ProductsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Se afiseaza lista tuturor produselor impreuna cu categoria 
        // din care fac parte
        // Pentru fiecare produs se afiseaza si userul care a postat produsul respectiv
        // [HttpGet] care se executa implicit
        [Authorize(Roles = "User,Colaborator,Admin")]
        public IActionResult Index()
        {
            var products = db.Products.Include("Category")
                                      .Include("User");

            // ViewBag.OriceDenumireSugestiva
            ViewBag.Products = products;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            return View();
        }

        // Se afiseaza un singur produs in functie de id-ul sau 
        // impreuna cu categoria din care face parte
        // In plus sunt preluate si toate comentariile asociate unui produs
        // Se afiseaza si userul care a postat produsul respectiv
        // [HttpGet] se executa implicit implicit
        [Authorize(Roles = "User,Colaborator,Admin")]
        public IActionResult Show(int id)
        {
            Product product = db.Products.Include("Category")
                                         .Include("Comments")
                                         .Include("User")
                                         .Include("Comments.User")
                              .Where(art => art.Id == id)
                              .First();

            SetAccessRights();

            return View(product);
        }

        // Se afiseaza formularul in care se vor completa datele unui produs
        // impreuna cu selectarea categoriei din care face parte
        // HttpGet implicit


        // Adaugarea unui comentariu asociat unui produs in baza de date
        // Toate rolurile pot adauga comentarii in baza de date
        [HttpPost]
        [Authorize(Roles = "User,Colaborator,Admin")]
        public IActionResult Show([FromForm] Comment comment)
        {
            comment.Date = DateTime.Now;

            // preluam Id-ul utilizatorului care posteaza comentariul
            comment.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();
                return Redirect("/Products/Show/" + comment.ProductId);
            }
            else
            {
                Product art = db.Products.Include("Category")
                                         .Include("User")
                                         .Include("Comments")
                                         .Include("Comments.User")
                               .Where(art => art.Id == comment.ProductId)
                               .First();

                //return Redirect("/Products/Show/" + comm.ProductId);

                SetAccessRights();

                return View(art);
            }
        }


        // Se afiseaza formularul in care se vor completa datele unui produs
        // impreuna cu selectarea categoriei din care face parte
        // Doar utilizatorii cu rolul de Colaborator si Admin pot adauga produse in platforma
        // [HttpGet] - care se executa implicit

        [Authorize(Roles = "Colaborator,Admin")]
        public IActionResult New()
        {
            Product product = new Product();

            product.Categ = GetAllCategories();

            return View(product);
        }

        // Se adauga produsul in baza de date
        // Doar utilizatorii cu rolul Colaborator si Admin pot adauga produse in platforma
        [HttpPost]
        [Authorize(Roles = "Colaborator,Admin")]
        public IActionResult New(Product Product)
        {
         

            // preluam Id-ul utilizatorului care posteaza produsul
            Product.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Products.Add(Product);
                db.SaveChanges();
                TempData["message"] = "produsul a fost adaugat";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                Product.Categ = GetAllCategories();
                return View(Product);
            }
        }

        // Se editeaza un produs existent in baza de date impreuna cu categoria din care face parte
        // Categoria se selecteaza dintr-un dropdown
        // Se afiseaza formularul impreuna cu datele aferente produsului din baza de date
        // Doar utilizatorii cu rolul de Colaborator si Admin pot edita produse
        // Adminii pot edita orice produs din baza de date
        // Colaboratorii pot edita doar produsele proprii (cele pe care ei le-au postat)
        // [HttpGet] - se executa implicit

        [Authorize(Roles = "Colaborator,Admin")]
        public IActionResult Edit(int id)
        {

            Product product = db.Products.Include("Category")
                                         .Where(art => art.Id == id)
                                         .First();

            product.Categ = GetAllCategories();

            if ((product.UserId == _userManager.GetUserId(User)) ||
                User.IsInRole("Admin"))
            {
                return View(product);
            }
            else
            {

                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui produs care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        // Se adauga produsul modificat in baza de date
        // Se verifica rolul utilizatorilor care au dreptul sa editeze (Colaborator si Admin)
        [HttpPost]
        [Authorize(Roles = "Colaborator,Admin")]
        public IActionResult Edit(int id, Product requestProduct)
        {
            Product product = db.Products.Find(id);

            if (ModelState.IsValid)
            {
                if ((product.UserId == _userManager.GetUserId(User))
                    || User.IsInRole("Admin"))
                {
                    product.Title = requestProduct.Title;
                    product.Description = requestProduct.Description;
                    product.Price = requestProduct.Price;
                    product.Stock = requestProduct.Stock;
                    product.CategoryId = requestProduct.CategoryId;
                    TempData["message"] = "produsul a fost modificat";
                    TempData["messageType"] = "alert-success";
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui produs care nu va apartine";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                requestProduct.Categ = GetAllCategories();
                return View(requestProduct);
            }
        }


        // Se sterge un produs din baza de date 
        // Utilizatorii cu rolul de Colaborator sau Admin pot sterge produse
        // Colaboratorii pot sterge doar produsele publicate de ei
        // Adminii pot sterge orice produs de baza de date

        [HttpPost]
        [Authorize(Roles = "Colaborator,Admin")]
        public ActionResult Delete(int id)
        {
            // Product Product = db.Products.Find(id);

            Product product = db.Products.Include("Comments")
                                         .Where(art => art.Id == id)
                                         .First();

            if ((product.UserId == _userManager.GetUserId(User))
                    || User.IsInRole("Admin"))
            {
                db.Products.Remove(product);
                db.SaveChanges();
                TempData["message"] = "produsul a fost sters";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti un produs care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        // Conditiile de afisare pentru butoanele de editare si stergere
        // butoanele aflate in view-uri
        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            if (User.IsInRole("Colaborator"))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.UserCurent = _userManager.GetUserId(User);

            ViewBag.EsteAdmin = User.IsInRole("Admin");
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            // generam o lista de tipul SelectListItem fara elemente
            var selectList = new List<SelectListItem>();

            // extragem toate categoriile din baza de date
            var categories = from cat in db.Categories
                             select cat;

            // iteram prin categorii
            foreach (var category in categories)
            {
                // adaugam in lista elementele necesare pentru dropdown
                // id-ul categoriei si denumirea acesteia
                selectList.Add(new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.CategoryName
                });
            }
            /* Sau se poate implementa astfel: 
             * 
            foreach (var category in categories)
            {
                var listItem = new SelectListItem();
                listItem.Value = category.Id.ToString();
                listItem.Text = category.CategoryName;

                selectList.Add(listItem);
             }*/


            // returnam lista de categorii
            return selectList;
        }

        // Metoda utilizata pentru exemplificarea Layout-ului
        // Am adaugat un nou Layout in Views -> Shared -> numit _LayoutNou.cshtml
        // Aceasta metoda are un View asociat care utilizeaza noul layout creat
        // in locul celui default generat de framework numit _Layout.cshtml
        public IActionResult IndexNou()
        {
            return View();
        }
    }
}

using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OnlineCleaningShop.Data;
using OnlineCleaningShop.Models;
using System.Globalization;

namespace OnlineCleaningShop.Controllers
{
    public class ProductsController : Controller
    {
        // PASUL 10: useri si roluri 

        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public ProductsController(
        ApplicationDbContext context,
        IWebHostEnvironment env,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _env = env;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Se afiseaza lista tuturor produselor impreuna cu categoria 
        // din care fac parte
        // Pentru fiecare produs se afiseaza si userul care a postat produsul respectiv
        // [HttpGet] care se executa implicit
        [AllowAnonymous]
        public IActionResult Index()
        {
            var products = db.Products.Include(p => p.Category)
                                    .Include(p => p.Reviews)
                                    //de pus conditia ca produsele sa fie aprobate de admin; in curand!
                                    .OrderBy(p => p.Name);

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            // MOTOR DE CAUTARE

            var search = Convert.ToString(HttpContext.Request.Query["search"])?.Trim(); // eliminam spatiile libere
            var sortBy = Convert.ToString(HttpContext.Request.Query["sortBy"]); // Parametru pentru sortare
            var sortOrder = Convert.ToString(HttpContext.Request.Query["sortOrder"]); // Ordine sortare

            // Verificăm dacă există parametrul de căutare în query
            if (!string.IsNullOrEmpty(search))
            {
                // Căutare în produse și review-uri
                // Căutare în produse și review-uri
                List<int> productIds = db.Products
                    .Where(p => (p.Name.Contains(search) || p.Description.Contains(search)) && // Căutăm în nume și descriere
                                !db.ProductRequests.Any(pr => pr.ProductId == p.Id && pr.Status != "Approved")) // Excludem produsele neaprobate
                    .Select(p => p.Id).ToList();

                List<int> productIdsOfReviewsWithSearchString = db.Reviews
                    .Where(r => r.Text.Contains(search) &&
                                !db.ProductRequests.Any(pr => pr.ProductId == r.ProductId && pr.Status != "Approved")) // Excludem produsele neaprobate
                    .Select(r => r.ProductId).ToList();

                List<int> mergedIds = productIds.Union(productIdsOfReviewsWithSearchString).ToList();

                // Filtrăm doar produsele care sunt aprobate și le ordonăm explicit
                products = db.Products
                    .Where(p => mergedIds.Contains(p.Id) &&
                                !db.ProductRequests.Any(pr => pr.ProductId == p.Id && pr.Status != "Approved"))
                    .OrderBy(p => p.Name); // Aplicăm ordonarea explicită aici

            }
            else
            {
                // Dacă nu se caută nimic, afișăm toate produsele
                products = db.Products
                        .Include(p => p.Category)
                        .Include(p => p.Reviews)
                        .Where(p => db.ProductRequests.Any(pr => pr.ProductId == p.Id && pr.Status == "Approved")) // Include doar produsele aprobate
                        .OrderBy(p => p.Name);
            }

            ViewBag.SearchString = search;

            switch (sortBy)
            {
                case "price":
                    products = sortOrder == "desc"
                        ? products.OrderByDescending(p => p.Price)
                        : products.OrderBy(p => p.Price);
                    break;

                case "stars":
                    products = sortOrder == "desc"
                        ? products.OrderByDescending(p => p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0)
                        : products.OrderBy(p => p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0);
                    break;

                default:
                    products = products.OrderBy(p => p.Name);
                    break;
            }


            // AFIȘARE PAGINATĂ

            int _perPage = 3; // Afișăm 3 produse pe pagină

            // Numărul total de produse rezultate după căutare (sau toate dacă nu se caută nimic)
            int totalItems = products.Count();

            // Preluăm pagina curentă din Query
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            // Offset-ul determină câte produse au fost afișate deja
            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            // Preluăm produsele pentru pagina curentă
            var paginatedProducts = products.Skip(offset).Take(_perPage);

            // Determinăm ultima pagină
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);

            // Trimitem lista paginată de produse către View
            ViewBag.Products = paginatedProducts;

            // DETERMINĂM BASE URL PENTRU PAGINARE
            if (!string.IsNullOrEmpty(search))
            {
                var queryParams = new Dictionary<string, string>
                {
                    { "search", search } //de pus si sortarea; in curand!
            };
                ViewBag.PaginationBaseUrl = "/Products/Index/?" + string.Join("&", queryParams
                    .Where(p => !string.IsNullOrEmpty(p.Value))
                    .Select(p => $"{p.Key}={p.Value}")) + "&page";
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Products/Index/?page";
            }

            return View();
        }

        // Se afiseaza un singur produs in functie de id-ul sau 
        // impreuna cu categoria din care face parte
        // In plus sunt preluate si toate comentariile asociate unui produs
        // Se afiseaza si userul care a postat produsul respectiv
        // [HttpGet] se executa implicit implicit
        [AllowAnonymous]
        public IActionResult Show(int id)
        {
            //verificam daca produsul are o cerere neaprobata; in curand!

            // Obținem produsul împreună cu toate relațiile necesare
            Product product = db.Products.Include("Category")
                                         .Include("Reviews")
                                         .Include("User")
                                         .Include("Reviews.User")
                              .Where(prod => prod.Id == id)
                              .First();

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.UserOrders = db.Orders
            .Where(o => o.UserId == _userManager.GetUserId(User))
            .ToList();

            SetAccessRights();

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            return View(product);
        }

        // Se plaseaza o comanda de catre un utilizator
        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult AddOrder([FromForm] OrderDetail orderDetail)
        {
            // Daca modelul este valid
            if (ModelState.IsValid)
            {
                // Verificam daca avem deja produsul in colectie
                if (db.OrderDetails
                    .Where(ab => ab.ProductId == orderDetail.ProductId)
                    .Where(ab => ab.OrderId == orderDetail.OrderId)
                    .Count() > 0)
                {
                    TempData["message"] = "Acest produs este deja adăugat în coș.";
                    TempData["messageType"] = "alert-danger";
                }
                else
                {
                    var product = db.Products.FirstOrDefault(p => p.Id == orderDetail.ProductId);

                    if (orderDetail.Quantity > product.Stock)
                    {
                        TempData["message"] = "Cantitatea selectată este mai mare decât stocul disponibil.";
                        TempData["messageType"] = "alert-danger";
                        return Redirect("/Products/Show/" + orderDetail.ProductId);
                    }

                    product.Stock -= orderDetail.Quantity;

                    // Adăugăm asocierea între produs și comandă 
                    db.OrderDetails.Add(orderDetail);
                    // Salvăm modificările
                    db.SaveChanges();

                    // Adăugăm un mesaj de succes
                    TempData["message"] = "Produsul a fost adăugat în coș.";
                    TempData["messageType"] = "alert-success";
                }
            }
            else
            {
                TempData["message"] = "Nu s-a putut adăuga produsul în coș.";
                TempData["messageType"] = "alert-danger";
            }

            // Ne întoarcem la pagina articolului
            return Redirect("/Products/Show/" + orderDetail.ProductId);
        }

        // Adaugarea unui review asociat unui produs in baza de date
        // Toate rolurile pot adauga review-uri in baza de date
        [HttpPost]
        [Authorize(Roles = "User,Colaborator,Admin")]
        public IActionResult Show([FromForm] Review review)
        {
            review.Date = DateTime.Now;

            // preluam Id-ul utilizatorului care posteaza review-ul
            review.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Reviews.Add(review);
                db.SaveChanges();
                return Redirect("/Products/Show/" + review.ProductId);
            }
            else
            {
                Product prod = db.Products.Include("Category")
                                         .Include("User")
                                         .Include("Reviews")
                                         .Include("Reviews.User")
                                         .Where(prod => prod.Id == review.ProductId)
                                         .First();

                SetAccessRights();

                return View(prod);
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

        //proceseaza datele introduse in formularul de mai sus
        [HttpPost]
        [Authorize(Roles = "Colaborator,Admin")]
        public async Task<IActionResult> New(Product product, IFormFile Image)
        {
            // Preluăm Id-ul utilizatorului care postează articolul
            product.UserId = _userManager.GetUserId(User);

            if (Image != null && Image.Length > 0)
            {
                // Verificăm extensia
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".mov" };
                var fileExtension = Path.GetExtension(Image.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ProductImage", "Fișierul trebuie să fie o imagine (jpg, jpeg, png, gif) sau un video (mp4,  mov).");
                    return View(product);
                }

                // Cale stocare
                var storagePath = Path.Combine(_env.WebRootPath, "images", Image.FileName);
                var databaseFileName = "/images/" + Image.FileName;

                // Salvare fișier
                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    await Image.CopyToAsync(fileStream);
                }

                ModelState.Remove(nameof(product.Image));
                product.Image = databaseFileName;
            }

            if (ModelState.IsValid)
            {
                // Adăugare produs în baza de date
                db.Products.Add(product);
                await db.SaveChangesAsync();

                // cream o cerere de aprobare si o adaugam in baza de date; in curand!


                TempData["message"] = "Produsul a fost adaugat!";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index", "Products");
            }

            product.Categ = GetAllCategories();
            return View(product);
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
            if (product == null)
            {
                TempData["message"] = "Produsul nu a fost găsit!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            // verificam daca produsul are cererea aprobată; in curand!

            product.Categ = GetAllCategories();

            if ((product.UserId == _userManager.GetUserId(User)) ||
                User.IsInRole("Admin"))
            {
                return View(product);
            }
            else
            {

                TempData["message"] = "Nu aveți dreptul să editați produse care nu vă aparțin!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        // Se adauga produsul modificat in baza de date
        // Se verifica rolul utilizatorilor care au dreptul sa editeze (Colaborator si Admin)
        [HttpPost]
        [Authorize(Roles = "Colaborator,Admin")]
        public async Task<IActionResult> Edit(int id, Product requestProduct, IFormFile Image)
        {
            // Găsim produsul în baza de date
            Product product = db.Products.Find(id);

            if (product == null)
            {
                TempData["message"] = "Produsul nu a fost gasit!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            // Gestionarea imaginii dacă este încărcată
            if (Image != null && Image.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".mov" };
                var fileExtension = Path.GetExtension(Image.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ProductImage", "Fișierul trebuie să fie o imagine (jpg, jpeg, png, gif) sau un video (mp4,  mov).");
                    return View(product);
                }

                // Cale stocare
                var storagePath = Path.Combine(_env.WebRootPath, "images", Image.FileName);
                var databaseFileName = "/images/" + Image.FileName;

                // Salvare fișier
                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    await Image.CopyToAsync(fileStream);
                }

                ModelState.Remove(nameof(product.Image));
                requestProduct.Image = databaseFileName;
            }

            // Validăm datele introduse
            if (ModelState.IsValid)
            {
                if ((product.UserId == _userManager.GetUserId(User)) || User.IsInRole("Admin"))
                {
                    // Creăm o cerere de aprobare; in curand!

                    product.Name = requestProduct.Name;
                    product.Description = requestProduct.Description;
                    product.Price = requestProduct.Price;
                    product.Stock = requestProduct.Stock;
                    product.CategoryId = requestProduct.CategoryId;
                    product.Image = requestProduct.Image;

                    // Adăugăm cererea în baza de date; in curand!
                    await db.SaveChangesAsync();

                    TempData["message"] = "Produsul a fost editat!";
                    TempData["messageType"] = "alert-success";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "Nu aveți dreptul să editați acest produs!";
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
            // Product product = db.Products.Find(id);

            Product product = db.Products.Include("Reviews")
                               .Where(prod => prod.Id == id)
                               .First();

            if ((product.UserId == _userManager.GetUserId(User))
                    || User.IsInRole("Admin"))
            {
                db.Products.Remove(product);
                db.SaveChanges();
                TempData["message"] = "Produsul a fost sters! ;(";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti acest produs!";
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

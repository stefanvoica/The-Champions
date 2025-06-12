using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnlineCleaningShop.Data;
using OnlineCleaningShop.Models;
using OnlineCleaningShop.Data;
using OnlineCleaningShop.Models;
using System.Globalization;
using static NuGet.Packaging.PackagingConstants;

namespace OnlineCleaningShop.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
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


        //afisare produse impreuna cu categoria
        //httpget implicit 
        [AllowAnonymous]
        public IActionResult Index()
        {
            var products = db.Products
                        .Include(p => p.Category)
                        .Include(p => p.Reviews)
                        .Where(p => db.ProductRequests.Any(pr => pr.ProductId == p.Id && pr.Status == RequestStatus.Approved))
                        .OrderBy(p => p.Name);

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.AlertType = TempData["messageType"];
            }

            ///MOTOR DE CAUTARE 
            var search = Convert.ToString(HttpContext.Request.Query["search"])?.Trim();
            var sortBy = Convert.ToString(HttpContext.Request.Query["sortBy"]); // Parametru pentru sortare
            var sortOrder = Convert.ToString(HttpContext.Request.Query["sortOrder"]); // Ordine sortare

            // Verificăm dacă există parametrul de căutare în query
            if (!string.IsNullOrEmpty(search))
            {
                // Căutare în produse și review-uri
                // Căutare în produse și review-uri
                List<int> productIds = db.Products
                    .Where(p => (p.Name.Contains(search) || p.Description.Contains(search)) && // Căutăm în nume și descriere
                                !db.ProductRequests.Any(pr => pr.ProductId == p.Id && pr.Status != RequestStatus.Approved)) // Excludem produsele neaprobate
                    .Select(p => p.Id).ToList();

                List<int> productIdsOfReviewsWithSearchString = db.Reviews
                    .Where(r => r.Text.Contains(search) &&
                                !db.ProductRequests.Any(pr => pr.ProductId == r.ProductId && pr.Status != RequestStatus.Approved)) // Excludem produsele neaprobate
                    .Select(r => r.ProductId).ToList();

                List<int> mergedIds = productIds.Union(productIdsOfReviewsWithSearchString).ToList();

                // Filtrăm doar produsele care sunt aprobate și le ordonăm explicit
                products = db.Products
                    .Where(p => mergedIds.Contains(p.Id) &&
                                !db.ProductRequests.Any(pr => pr.ProductId == p.Id && pr.Status != RequestStatus.Approved))
                    .OrderBy(p => p.Name); // Aplicăm ordonarea explicită aici

            }
            else
            {
                // Dacă nu se caută nimic, afișăm toate produsele
                products = db.Products
                        .Include(p => p.Category)
                        .Include(p => p.Reviews)
                        .Where(p => db.ProductRequests.Any(pr => pr.ProductId == p.Id && pr.Status == RequestStatus.Approved)) // Include doar produsele aprobate
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
                { "search", search },
                { "sortBy", sortBy },
                { "sortOrder", sortOrder }
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

        [AllowAnonymous]
        public ActionResult Show(int id)
        {
            // Verificăm dacă produsul are o cerere neaprobată
            var productRequest = db.ProductRequests.FirstOrDefault(pr => pr.ProductId == id && pr.Status != RequestStatus.Approved);

            if (productRequest != null)
            {
                // Produsul nu este aprobat
                TempData["message"] = "Acest produs nu este aprobat și nu poate fi vizualizat.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            // Obținem produsul împreună cu toate relațiile necesare
            Product product = db.Products
             .Include("Category")
             .Include("Reviews")
             .Include("User")
             .Include("Reviews.User")
             .Where(prod => prod.Id == id)
             .First();

            if (product == null)
            {
                return NotFound();
            }

            // Obținem comenzile utilizatorului curent (dropdown pentru adăugare)
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

        [HttpPost]
        [Authorize(Roles = "User,Colaborator,Admin")]
        public IActionResult AddOrder([FromForm] OrderDetail orderDetail)
        {
            // Daca modelul este valid
            if (ModelState.IsValid)
            {
                // Verificăm dacă produsul este aprobat
                var productRequest = db.ProductRequests.FirstOrDefault(pr => pr.ProductId == orderDetail.ProductId && pr.Status != RequestStatus.Approved);
                if (productRequest != null)
                {
                    TempData["message"] = "Acest produs nu este aprobat și nu poate fi adăugat în coș.";
                    TempData["messageType"] = "alert-danger";
                    return Redirect("/Products/Show/" + orderDetail.ProductId);
                }

                // Verificam daca avem deja produsul in colectie
                if (db.OrderDetails
                    .Where(ab => ab.ProductId == orderDetail.ProductId)
                    .Where(ab => ab.OrderId == orderDetail.OrderId)
                    .Count() > 0)
                {
                    //Crestem cantitatea din cos
                    var existingOrderDetail = db.OrderDetails
                        .FirstOrDefault(ab => ab.ProductId == orderDetail.ProductId && ab.OrderId == orderDetail.OrderId);
                    existingOrderDetail.Quantity += orderDetail.Quantity;
                    db.SaveChanges();
                    TempData["message"] = "Cantitatea produsului a fost actualizată în coș.";
                    TempData["messageType"] = "alert-success";

                    //TempData["message"] = "Acest produs este deja adăugat în coș.";
                    //TempData["messageType"] = "alert-danger";
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



        //se afiseaza formularul in care completam datele produsului
        //impreuna cu selectarea categoriei
        //httpget implicit
        [Authorize(Roles = "Colaborator,Admin")]
        public IActionResult New()
        {
            Product product = new Product();

            product.Categ = GetAllCategories();

            return View(product);
        }

        //proceseaza datele introduse in formularul de mai sus
        [Authorize(Roles = "Colaborator,Admin")]
        [HttpPost]
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
                    ModelState.AddModelError("ArticleImage", "Fișierul trebuie să fie o imagine (jpg, jpeg, png, gif) sau un video (mp4,  mov).");
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

                // Creăm o cerere de aprobare
                var productRequest = new ProductRequest
                {
                    ProductId = product.Id,
                    UserId = _userManager.GetUserId(User),
                    Status = RequestStatus.Pending // Cererea trebuie aprobată
                };

                // Adăugăm cererea în baza de date
                db.ProductRequests.Add(productRequest);
                await db.SaveChangesAsync();

                TempData["message"] = "Cererea de adăugare a fost trimisă pentru aprobare!";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index", "Products");
            }

            product.Categ = GetAllCategories();
            return View(product);
        }

        public IActionResult Edit(int id)
        {
            Product product = db.Products.Include("Category")
                               .Where(prod => prod.Id == id)
                               .First();


            if (product == null)
            {
                TempData["message"] = "Produsul nu a fost găsit!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            var productRequest = db.ProductRequests.FirstOrDefault(pr => pr.ProductId == id && pr.Status == RequestStatus.Approved);

            if (productRequest == null)
            {
                TempData["message"] = "Cererea de editare nu a fost aprobată!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            product.Categ = GetAllCategories();

            if ((product.UserId == _userManager.GetUserId(User)) || User.IsInRole("Admin"))
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

        [HttpPost]
        [Authorize(Roles = "Colaborator,Admin")]
        public async Task<IActionResult> Edit(int id, Product requestProduct, IFormFile Image)
        {
            // Găsim produsul în baza de date
            Product product = db.Products.Find(id);

            if (product == null)
            {
                TempData["message"] = "Produsul nu a fost găsit!";
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
                    ModelState.AddModelError("ArticleImage", "Fișierul trebuie să fie o imagine (jpg, jpeg, png, gif) sau un video (mp4,  mov).");
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
                    // Creăm o cerere de aprobare
                    var productRequest = new ProductRequest
                    {
                        ProductId = product.Id,
                        UserId = _userManager.GetUserId(User),
                        Status = RequestStatus.Pending, // Cererea trebuie aprobată
                        Product = new Product
                        {
                            Name = requestProduct.Name,
                            Description = requestProduct.Description,
                            Price = requestProduct.Price,
                            Stock = requestProduct.Stock,
                            CategoryId = requestProduct.CategoryId,
                            Image = requestProduct.Image
                        }
                    };

                    // Adăugăm cererea în baza de date
                    db.ProductRequests.Add(productRequest);
                    await db.SaveChangesAsync();

                    TempData["message"] = "Cererea de editare a fost trimisă pentru aprobare!";
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



        [HttpPost]
        [Authorize(Roles = "Colaborator,Admin")]
        public ActionResult Delete(int id)
        {
            //Product product = db.Products.Find(id);

            Product product = db.Products.Include("Reviews")
                               .Where(prod => prod.Id == id)
                               .First();

            if ((product.UserId == _userManager.GetUserId(User)) || User.IsInRole("Admin"))
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

        public IActionResult IndexNou()
        {
            return View();
        }
    }
}
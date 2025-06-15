using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using OnlineCleaningShop.Controllers;
using OnlineCleaningShop.Data;
using OnlineCleaningShop.Models;
using System.IO;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;

namespace OnlineCleaningShop.Automated_Test
{
    public class ProductsControllerTests
    {
        [Fact]
        public async Task New_ValidProduct_SavesProductAndProductRequest()
        {
            // 1. Setup in-memory DB
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("ProductAddTestDb")
                .Options;
            using var context = new ApplicationDbContext(options);

            // Populează categoriile (altfel GetAllCategories() returnează gol)
            context.Categories.Add(new Category { Id = 1, CategoryName = "Test" });
            context.SaveChanges();

            // 2. Setup mocks
            var env = new Mock<IWebHostEnvironment>();
            var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var imagesPath = Path.Combine(webRootPath, "images");
            Directory.CreateDirectory(imagesPath); // creează folderul necesar
            env.Setup(e => e.WebRootPath).Returns(webRootPath);

            var store = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
            userManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user123");

            var roleStore = new Mock<IRoleStore<IdentityRole>>();
            var roleManager = new Mock<RoleManager<IdentityRole>>(roleStore.Object, null, null, null, null);

            var controller = new ProductsController(context, env.Object, userManager.Object, roleManager.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };
            controller.ModelState.Clear();
            controller.TempData = new Mock<ITempDataDictionary>().Object;

            // 3. Pregătim produsul
            var product = new Product
            {
                Name = "Produs Test",
                Description = "Descriere",
                Price = 99,
                Stock = 5,
                CategoryId = 1
            };

            // 4. Simulăm imagine
            var fileMock = new Mock<IFormFile>();
            var content = "fake image content";
            var fileName = "image.png";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;
            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(ms.Length);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default)).Returns((Stream s, System.Threading.CancellationToken _) => ms.CopyToAsync(s));

            // 5. Execută metoda
            var result = await controller.New(product, fileMock.Object) as RedirectToActionResult;

            // 6. Verificări
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Products", result.ControllerName);

            var savedProduct = context.Products.FirstOrDefault();
            Assert.NotNull(savedProduct);
            Assert.Equal("Produs Test", savedProduct.Name);
            Assert.Equal("user123", savedProduct.UserId);

            var request = context.ProductRequests.FirstOrDefault();
            Assert.NotNull(request);
            Assert.Equal(savedProduct.Id, request.ProductId);
            Assert.Equal("user123", request.UserId);
        }
    }
}

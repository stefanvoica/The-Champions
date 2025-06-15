using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using OnlineCleaningShop.Controllers;
using OnlineCleaningShop.Data;
using OnlineCleaningShop.Models;
using System.Security.Claims;

namespace OnlineCleaningShop.Automated_Test
{
    public class OrdersControllerTests
    {
        [Fact]
        public void New_ValidOrder_SavesToDatabase_AndRedirectsToProducts()
        {
            // 1. Mock pentru UserManager
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<UserManager<ApplicationUser>>(userStore.Object, null, null, null, null, null, null, null, null);
            userManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user123");

            // 2. Mock pentru RoleManager (chiar dacă nu se folosește direct aici, constructorul cere)
            var roleStore = new Mock<IRoleStore<IdentityRole>>();
            var roleManager = new Mock<RoleManager<IdentityRole>>(roleStore.Object, null, null, null, null);

            // 3. Setup DbContext in-memory
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("OrdersTestDb")
                .Options;

            using var context = new ApplicationDbContext(options);

            // 4. Controller
            var controller = new OrdersController(context, userManager.Object, roleManager.Object);
            var tempData = new Mock<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionary>();
            controller.TempData = tempData.Object;

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };

            // 5. Simulează comanda
            var order = new Order
            {
                Name = "Test Cos",
                DeliveryMethod = DeliveryMethod.WarehousePickup   

            };

            // 6. Apelează metoda
            var result = controller.New(order) as RedirectToActionResult;

            // 7. Verificări
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Products", result.ControllerName);

            var savedOrder = context.Orders.FirstOrDefault();
            Assert.NotNull(savedOrder);
            Assert.Equal("user123", savedOrder.UserId);
            Assert.Equal("Test Cos", savedOrder.Name);
        }
    }
}

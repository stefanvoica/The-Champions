using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using OnlineCleaningShop.Controllers;
using OnlineCleaningShop.Data;
using OnlineCleaningShop.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace OnlineCleaningShop.Automated_Test
{
    public class CategoriesControllerTests
    {
        [Fact]
        public void New_ValidCategory_SavesToDatabaseAndRedirects()
        {
            // 1. Setup DB
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("CategoryAddTestDb")
                .Options;
            using var context = new ApplicationDbContext(options);

            // 2. Setup mocks
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<UserManager<ApplicationUser>>(userStore.Object, null, null, null, null, null, null, null, null);
            var roleStore = new Mock<IRoleStore<IdentityRole>>();
            var roleManager = new Mock<RoleManager<IdentityRole>>(roleStore.Object, null, null, null, null);

            var controller = new CategoriesController(context, userManager.Object, roleManager.Object);

            // 3. Simulează TempData
            var tempData = new Mock<ITempDataDictionary>();
            controller.TempData = tempData.Object;

            // 4. Creează o categorie validă
            var category = new Category { CategoryName = "TestCategory" };

            // 5. Execută metoda
            var result = controller.New(category) as RedirectToActionResult;

            // 6. Verificări
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);

            var savedCategory = context.Categories.FirstOrDefault();
            Assert.NotNull(savedCategory);
            Assert.Equal("TestCategory", savedCategory.CategoryName);
        }

        [Fact]
        public void New_InvalidModel_ReturnsViewWithModel()
        {
            // 1. Setup DB
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("CategoryInvalidTestDb")
                .Options;
            using var context = new ApplicationDbContext(options);

            // 2. Controller setup
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<UserManager<ApplicationUser>>(userStore.Object, null, null, null, null, null, null, null, null);
            var roleStore = new Mock<IRoleStore<IdentityRole>>();
            var roleManager = new Mock<RoleManager<IdentityRole>>(roleStore.Object, null, null, null, null);

            var controller = new CategoriesController(context, userManager.Object, roleManager.Object);
            controller.ModelState.AddModelError("CategoryName", "Required");

            // 3. Categorie invalidă
            var category = new Category();

            // 4. Execută
            var result = controller.New(category) as ViewResult;

            // 5. Verificări
            Assert.NotNull(result);
            Assert.Equal(category, result.Model);
        }

        [Fact]
        public async Task RevokeUserRoles_ValidUser_RevokesRolesAndLocksAccount()
        {
            // 1. Setup In-Memory DB
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("RevokeUserRolesTestDb")
                .Options;
            using var context = new ApplicationDbContext(options);

            // 2. Add user to DB
            var user = new ApplicationUser { Id = "user-id", UserName = "testuser", Email = "test@test.com" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // 3. Setup UserManager + RoleManager Mocks
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<UserManager<ApplicationUser>>(userStore.Object, null, null, null, null, null, null, null, null);
            var roleStore = new Mock<IRoleStore<IdentityRole>>();
            var roleManager = new Mock<RoleManager<IdentityRole>>(roleStore.Object, null, null, null, null);

            userManager.Setup(um => um.FindByIdAsync("user-id")).ReturnsAsync(user);
            userManager.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User", "Admin" });
            userManager.Setup(um => um.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>())).ReturnsAsync(IdentityResult.Success);
            userManager.Setup(um => um.UpdateAsync(user)).Callback<ApplicationUser>(u =>
            {
                u.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
            }).ReturnsAsync(IdentityResult.Success);

            // 4. Creează controller
            var controller = new UsersController(context, userManager.Object, roleManager.Object);
            controller.TempData = new Mock<ITempDataDictionary>().Object;

            // 5. Execută
            var result = await controller.RevokeUserRoles("user-id") as RedirectToActionResult;

            // 6. Verificări
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.NotNull(user.LockoutEnd);
            Assert.True(user.LockoutEnd > DateTimeOffset.UtcNow.AddYears(99));
        }


    }
}

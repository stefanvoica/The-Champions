using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using OnlineCleaningShop.Controllers;
using OnlineCleaningShop.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineCleaningShop.Automated_Test
{
    public class UsersControllerTests
    {
        [Fact]
        public async Task Edit_ValidData_UpdatesUserAndRedirects()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "user-id",
                Email = "old@example.com",
                UserName = "olduser"
            };

            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<UserManager<ApplicationUser>>(userStore.Object, null, null, null, null, null, null, null, null);
            var roleStore = new Mock<IRoleStore<IdentityRole>>();
            var roleManager = new Mock<RoleManager<IdentityRole>>(roleStore.Object, null, null, null, null);

            userManager.Setup(um => um.FindByIdAsync("user-id")).ReturnsAsync(user);
            userManager.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });
            userManager.Setup(um => um.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>())).ReturnsAsync(IdentityResult.Success);
            userManager.Setup(um => um.AddToRoleAsync(user, "Admin")).ReturnsAsync(IdentityResult.Success);
            userManager.Setup(um => um.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var controller = new UsersController(null, userManager.Object, roleManager.Object);
            controller.TempData = new Mock<ITempDataDictionary>().Object;

            // Act
            var result = await controller.Edit("user-id", "new@example.com", "newuser", "Admin") as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("new@example.com", user.Email);
            Assert.Equal("newuser", user.UserName);
        }
    }
}

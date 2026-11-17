using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Moq;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Domain.Constants;
using RestaurantMS.Web.Models;
using RestaurantMS.Web.Services;
using RestaurantMS.Web.ViewComponents;
using Xunit;

namespace RestaurantMS.Integration.Tests;

public class NavMenuAndUserTests
{
    [Fact]
    public void WebCurrentUser_WhenUserIsOwner_ReturnsAllPermissions()
    {
        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "admin"),
            new(ClaimTypes.Name, "System Administrator"),
            new(ClaimTypes.Role, "Owner")
        };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

        var mockAccessor = new Mock<IHttpContextAccessor>();
        mockAccessor.Setup(a => a.HttpContext).Returns(httpContext);

        var currentUser = new WebCurrentUser(mockAccessor.Object);

        currentUser.IsAuthenticated.Should().BeTrue();
        currentUser.UserId.Should().Be("admin");
        currentUser.UserName.Should().Be("System Administrator");
        currentUser.Roles.Should().Contain("Owner");
        currentUser.Permissions.Should().Contain(Permissions.Dashboard.View);
        currentUser.Permissions.Should().Contain(Permissions.Tables.View);
        currentUser.Permissions.Should().Contain(Permissions.Billing.View);
    }

    [Fact]
    public async Task NavMenuViewComponent_WhenUserIsOwner_ReturnsAllMenuItems()
    {
        var mockUser = new Mock<ICurrentUser>();
        mockUser.Setup(u => u.Roles).Returns(new[] { "Owner" });
        mockUser.Setup(u => u.Permissions).Returns(Permissions.GetAll());

        var mockFeatureService = new Mock<IFeatureService>();
        mockFeatureService.Setup(f => f.IsEnabledAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var component = new NavMenuViewComponent(mockUser.Object, mockFeatureService.Object);
        var result = await component.InvokeAsync();

        var viewResult = result.Should().BeOfType<ViewViewComponentResult>().Subject;
        var model = viewResult.ViewData?.Model.Should().BeAssignableTo<IReadOnlyList<NavItem>>().Subject;

        model.Should().NotBeNull();
        model!.Count.Should().Be(11);
        model.Should().Contain(i => i.Controller == "Dashboard");
        model.Should().Contain(i => i.Controller == "Tables");
        model.Should().Contain(i => i.Controller == "Billing");
        model.Should().Contain(i => i.Controller == "Kitchen");
        model.Should().Contain(i => i.Controller == "OnlineOrders");
        model.Should().Contain(i => i.Controller == "Inventory");

        var mastersItem = model.FirstOrDefault(i => i.Controller == "Masters");
        mastersItem.Should().NotBeNull();
        mastersItem!.Children.Count.Should().Be(14);
        mastersItem.Children.Should().Contain(c => c.Controller == "Menu");
        mastersItem.Children.Should().Contain(c => c.Controller == "Customers");
        mastersItem.Children.Should().Contain(c => c.Controller == "Inventory");
        mastersItem.Children.Should().Contain(c => c.Controller == "Staff");
    }
}

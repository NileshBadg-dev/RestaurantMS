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
        currentUser.Permissions.Should().Contain(Permissions.Masters.View);
    }

    [Fact]
    public async Task NavMenuViewComponent_WhenUserIsOwner_ReturnsGroupedRestructuredMenu()
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

        // 1. Standalone Dashboard
        model!.Should().Contain(i => i.Controller == "Dashboard" && !i.IsSectionHeader);

        // 2. OPERATIONS Section
        model.Should().Contain(i => i.IsSectionHeader && i.TitleKey == "Nav_Section_Operations");
        model.Should().Contain(i => i.Controller == "Tables");
        model.Should().Contain(i => i.Controller == "Parcel");
        model.Should().Contain(i => i.Controller == "Orders");
        model.Should().Contain(i => i.Controller == "Kitchen");
        model.Should().Contain(i => i.Controller == "Billing");
        model.Should().Contain(i => i.Controller == "Reservations");

        // 3. Masters Single Entry (no children)
        var mastersItem = model!.FirstOrDefault(i => i.Controller == "Masters");
        mastersItem.Should().NotBeNull();
        mastersItem!.Children.Should().BeEmpty();

        // 4. Inventory Own Group (7 items)
        var inventoryGroup = model!.FirstOrDefault(i => i.Controller == "Inventory" && i.Children.Count > 0);
        inventoryGroup.Should().NotBeNull();
        inventoryGroup!.Children.Count.Should().Be(7);
        inventoryGroup.Children.Should().Contain(c => c.Action == "Index");
        inventoryGroup.Children.Should().Contain(c => c.Action == "Purchase");
        inventoryGroup.Children.Should().Contain(c => c.Action == "Adjustment");
        inventoryGroup.Children.Should().Contain(c => c.Action == "Wastage");
        inventoryGroup.Children.Should().Contain(c => c.Action == "Consumption");
        inventoryGroup.Children.Should().Contain(c => c.Action == "LowStock");
        inventoryGroup.Children.Should().Contain(c => c.Action == "Ledger");

        // 5. Staff Own Group
        var staffGroup = model!.FirstOrDefault(i => i.Controller == "Staff" && i.Children.Count > 0);
        staffGroup.Should().NotBeNull();
        staffGroup!.Children.Count.Should().Be(3);
        staffGroup.Children.Should().Contain(c => c.Action == "Index");
        staffGroup.Children.Should().Contain(c => c.Action == "Attendance");
        staffGroup.Children.Should().Contain(c => c.Action == "SalaryReport");

        // 6. Accounts Own Group
        var accountsGroup = model!.FirstOrDefault(i => i.Controller == "Accounts" && i.Children.Count > 0);
        accountsGroup.Should().NotBeNull();
        accountsGroup!.Children.Count.Should().Be(10);
        accountsGroup.Children.Should().Contain(c => c.TitleKey == "Nav_Vouchers");
        accountsGroup.Children.Should().Contain(c => c.TitleKey == "Nav_CustomerLedger");
        accountsGroup.Children.Should().Contain(c => c.TitleKey == "Nav_StaffAdvances");
        accountsGroup.Children.Should().Contain(c => c.TitleKey == "Nav_DayBook");
        accountsGroup.Children.Should().Contain(c => c.TitleKey == "Nav_CashBook");
        accountsGroup.Children.Should().Contain(c => c.TitleKey == "Nav_GeneralLedger");
        accountsGroup.Children.Should().Contain(c => c.TitleKey == "Nav_TrialBalance");
        accountsGroup.Children.Should().Contain(c => c.TitleKey == "Nav_ProfitLoss");
        accountsGroup.Children.Should().Contain(c => c.TitleKey == "Nav_BalanceSheet");
        accountsGroup.Children.Should().Contain(c => c.TitleKey == "Nav_Expenses");

        // 7. Reports & Day Closing
        model.Should().Contain(i => i.Controller == "Reports");
        model.Should().Contain(i => i.Controller == "DayClosing");

        // 8. ADMINISTRATION Section
        model.Should().Contain(i => i.IsSectionHeader && i.TitleKey == "Nav_Section_Administration");
        model.Should().Contain(i => i.Controller == "Settings");
        model.Should().Contain(i => i.Controller == "Users");
        model.Should().Contain(i => i.Controller == "Roles");
        model.Should().Contain(i => i.Controller == "FeatureToggles");
    }

    [Fact]
    public async Task NavMenuViewComponent_WhenUserIsWaiter_HidesRestrictedSections()
    {
        var mockUser = new Mock<ICurrentUser>();
        mockUser.Setup(u => u.Roles).Returns(new[] { "Waiter" });
        mockUser.Setup(u => u.Permissions).Returns(new[]
        {
            Permissions.Dashboard.View,
            Permissions.Tables.View,
            Permissions.Parcel.View,
            Permissions.Orders.View
        });

        var mockFeatureService = new Mock<IFeatureService>();
        mockFeatureService.Setup(f => f.IsEnabledAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var component = new NavMenuViewComponent(mockUser.Object, mockFeatureService.Object);
        var result = await component.InvokeAsync();

        var viewResult = result.Should().BeOfType<ViewViewComponentResult>().Subject;
        var model = viewResult.ViewData?.Model.Should().BeAssignableTo<IReadOnlyList<NavItem>>().Subject;

        model.Should().NotBeNull();
        model!.Should().NotContain(i => i.Controller == "Accounts");
        model.Should().NotContain(i => i.Controller == "Settings");
        model.Should().NotContain(i => i.Controller == "Users");
        model.Should().NotContain(i => i.Controller == "Roles");
        model.Should().NotContain(i => i.Controller == "FeatureToggles");
        model.Should().NotContain(i => i.IsSectionHeader && i.TitleKey == "Nav_Section_Administration");
    }

    [Fact]
    public async Task NavMenuViewComponent_WhenInventoryToggleDisabled_HidesInventoryGroup()
    {
        var mockUser = new Mock<ICurrentUser>();
        mockUser.Setup(u => u.Roles).Returns(new[] { "Owner" });
        mockUser.Setup(u => u.Permissions).Returns(Permissions.GetAll());

        var mockFeatureService = new Mock<IFeatureService>();
        mockFeatureService.Setup(f => f.IsEnabledAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string key, CancellationToken _) => !string.Equals(key, FeatureKeys.Inventory, StringComparison.OrdinalIgnoreCase));

        var component = new NavMenuViewComponent(mockUser.Object, mockFeatureService.Object);
        var result = await component.InvokeAsync();

        var viewResult = result.Should().BeOfType<ViewViewComponentResult>().Subject;
        var model = viewResult.ViewData?.Model.Should().BeAssignableTo<IReadOnlyList<NavItem>>().Subject;

        model.Should().NotBeNull();
        model!.Should().NotContain(i => i.Controller == "Inventory");
    }
}

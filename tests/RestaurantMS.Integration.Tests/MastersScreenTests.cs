using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Domain.Constants;
using RestaurantMS.Web.Controllers;
using RestaurantMS.Web.Models;
using RestaurantMS.Web.SampleData;
using Xunit;

namespace RestaurantMS.Integration.Tests;

public class MastersScreenTests
{
    private readonly IMastersUiData _mastersUiData = new SampleMastersUiData();

    [Fact]
    public async Task MastersUiData_GetTabCountsAsync_ReturnsExpectedAccurateCounts()
    {
        var counts = await _mastersUiData.GetTabCountsAsync();

        counts.Should().NotBeNull();
        counts.FloorsCount.Should().Be(3);
        counts.TablesCount.Should().Be(9);
        counts.CategoriesCount.Should().Be(5);
        counts.MenuItemsCount.Should().Be(7);
        counts.CustomersCount.Should().Be(5);
        counts.SuppliersCount.Should().Be(4);
        counts.DesignationsCount.Should().Be(5);
        counts.ShiftsCount.Should().Be(3);
        counts.TaxUnitsCount.Should().Be(6);
        counts.PaymentModesCount.Should().Be(5);
        counts.ExpenseHeadsCount.Should().Be(6);
        counts.HotelInfoBadge.Should().Be("GST & Info");
    }

    [Fact]
    public async Task MastersUiData_GetMenuItemsAsync_WhenCategoryFiltered_ReturnsMatchingItems()
    {
        var allItems = await _mastersUiData.GetMenuItemsAsync();
        allItems.Count.Should().Be(7);

        var starters = await _mastersUiData.GetMenuItemsAsync("Starters");
        starters.Count.Should().Be(1);
        starters[0].Name.Should().Be("Paneer Tikka");

        var mainCourse = await _mastersUiData.GetMenuItemsAsync("Main Course");
        mainCourse.Count.Should().Be(2);

        var beverages = await _mastersUiData.GetMenuItemsAsync("Beverages");
        beverages.Count.Should().Be(2);

        var desserts = await _mastersUiData.GetMenuItemsAsync("Desserts");
        desserts.Count.Should().Be(1);
        desserts[0].Name.Should().Be("Gulab Jamun (2 Pcs)");
    }

    [Fact]
    public async Task MastersController_Index_DefaultTab_ReturnsMenuItemsTab()
    {
        var mockUser = new Mock<ICurrentUser>();
        mockUser.Setup(u => u.Roles).Returns(new[] { "Owner" });
        mockUser.Setup(u => u.Permissions).Returns(Permissions.GetAll());

        var mockFeature = new Mock<IFeatureService>();
        mockFeature.Setup(f => f.IsEnabledAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var controller = new MastersController(_mastersUiData, mockUser.Object, mockFeature.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var result = await controller.Index();

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<MastersViewModel>().Subject;

        model.ActiveTab.Should().Be("menu-items");
        model.Tabs.Count.Should().Be(12);
        model.MenuItems.Should().NotBeNull();
        model.MenuItems!.Count.Should().Be(7);
    }

    [Fact]
    public async Task MastersController_TabContent_SuppliersTab_ReturnsPartialViewWithSuppliers()
    {
        var mockUser = new Mock<ICurrentUser>();
        mockUser.Setup(u => u.Roles).Returns(new[] { "Owner" });
        mockUser.Setup(u => u.Permissions).Returns(Permissions.GetAll());

        var mockFeature = new Mock<IFeatureService>();
        mockFeature.Setup(f => f.IsEnabledAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var controller = new MastersController(_mastersUiData, mockUser.Object, mockFeature.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var result = await controller.TabContent("suppliers");

        var partialResult = result.Should().BeOfType<PartialViewResult>().Subject;
        partialResult.ViewName.Should().Be("_TabContent");
        var model = partialResult.Model.Should().BeOfType<MastersViewModel>().Subject;

        model.ActiveTab.Should().Be("suppliers");
        model.Suppliers.Should().NotBeNull();
        model.Suppliers!.Count.Should().Be(4);
    }

    [Fact]
    public void MenuController_OldRoutes_RedirectPermanentToMastersTab()
    {
        var mockMenuUi = new Mock<IMenuUiData>();
        var controller = new MenuController(mockMenuUi.Object);

        // 1. Categories
        var catResult = controller.Categories().Should().BeOfType<RedirectToActionResult>().Subject;
        catResult.Permanent.Should().BeTrue();
        catResult.ControllerName.Should().Be("Masters");
        catResult.ActionName.Should().Be("Index");
        catResult.RouteValues!["tab"].Should().Be("categories");

        // 2. Items
        var itemsResult = controller.Items(null).Should().BeOfType<RedirectToActionResult>().Subject;
        itemsResult.Permanent.Should().BeTrue();
        itemsResult.ControllerName.Should().Be("Masters");
        itemsResult.ActionName.Should().Be("Index");
        itemsResult.RouteValues!["tab"].Should().Be("menu-items");

        // 3. Index
        var indexResult = controller.Index().Should().BeOfType<RedirectToActionResult>().Subject;
        indexResult.Permanent.Should().BeTrue();
        indexResult.ControllerName.Should().Be("Masters");
        indexResult.ActionName.Should().Be("Index");
        indexResult.RouteValues!["tab"].Should().Be("menu-items");
    }

    [Fact]
    public void CustomersController_Index_RedirectsPermanentToMastersCustomersTab()
    {
        var mockCustomersUi = new Mock<ICustomersUiData>();
        var controller = new CustomersController(mockCustomersUi.Object);

        var result = controller.Index().Should().BeOfType<RedirectToActionResult>().Subject;
        result.Permanent.Should().BeTrue();
        result.ControllerName.Should().Be("Masters");
        result.ActionName.Should().Be("Index");
        result.RouteValues!["tab"].Should().Be("customers");
    }

    [Fact]
    public void InventoryController_Suppliers_RedirectsPermanentToMastersSuppliersTab()
    {
        var controller = new InventoryController(
            new Mock<IInventoryService>().Object,
            new Mock<IMastersUiData>().Object,
            new Mock<IFileStorage>().Object,
            new Mock<ISettingService>().Object,
            new Mock<IAuditUiData>().Object);

        var result = controller.Suppliers().Should().BeOfType<RedirectToActionResult>().Subject;
        result.Permanent.Should().BeTrue();
        result.ControllerName.Should().Be("Masters");
        result.ActionName.Should().Be("Index");
        result.RouteValues!["tab"].Should().Be("suppliers");

        var formRedirect = controller.SupplierForm().Should().BeOfType<RedirectToActionResult>().Subject;
        formRedirect.Permanent.Should().BeTrue();
        formRedirect.ControllerName.Should().Be("Masters");
        formRedirect.RouteValues!["tab"].Should().Be("suppliers");
    }
}

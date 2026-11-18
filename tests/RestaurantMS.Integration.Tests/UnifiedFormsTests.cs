using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Primitives;
using Moq;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Domain.Constants;
using RestaurantMS.Web.Controllers;
using RestaurantMS.Web.Models;
using RestaurantMS.Web.SampleData;
using Xunit;

namespace RestaurantMS.Integration.Tests;

public class UnifiedFormsTests
{
    [Theory]
    [InlineData("floors")]
    [InlineData("tables")]
    [InlineData("categories")]
    [InlineData("customers")]
    [InlineData("suppliers")]
    [InlineData("designations")]
    [InlineData("shifts")]
    [InlineData("tax-units")]
    [InlineData("payment-modes")]
    [InlineData("expense-heads")]
    public void MastersController_DrawerForm_ReturnsShortFormWithAtMost8Fields(string tab)
    {
        var mockUser = new Mock<ICurrentUser>();
        mockUser.Setup(u => u.Roles).Returns(new[] { "Owner" });
        mockUser.Setup(u => u.Permissions).Returns(Permissions.GetAll());

        var mockFeature = new Mock<IFeatureService>();
        var controller = new MastersController(new SampleMastersUiData(), mockUser.Object, mockFeature.Object);

        var result = controller.DrawerForm(tab).Should().BeOfType<PartialViewResult>().Subject;
        result.ViewName.Should().Be("_MasterDrawerForm");
        var model = result.Model.Should().BeOfType<MasterDrawerFormViewModel>().Subject;

        model.Tab.Should().Be(tab);
        model.Fields.Count.Should().BeInRange(3, 8);
    }

    [Fact]
    public async Task MastersController_SaveMasterEntry_PersistsAndReturnsHighlightKey()
    {
        var mockUser = new Mock<ICurrentUser>();
        var mockFeature = new Mock<IFeatureService>();
        var uiData = new SampleMastersUiData();
        var controller = new MastersController(uiData, mockUser.Object, mockFeature.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Form = new FormCollection(new Dictionary<string, StringValues>
        {
            ["Name"] = "Garden Courtyard",
            ["Code"] = "GC-01",
            ["TableCount"] = "5",
            ["SeatingCapacity"] = "20",
            ["IsActive"] = "true"
        });
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var jsonResult = (await controller.SaveMasterEntry("floors")).Should().BeOfType<JsonResult>().Subject;
        var payload = System.Text.Json.JsonSerializer.Serialize(jsonResult.Value);
        payload.Should().Contain("\"isSuccess\":true");
        payload.Should().Contain("Garden Courtyard");

        var floors = await uiData.GetFloorsAsync();
        floors.Should().Contain(f => f.Name == "Garden Courtyard");
    }

    [Fact]
    public void MenuController_ItemForm_SupportsSaveAndSaveAndNew()
    {
        var mockMenuUi = new Mock<IMenuUiData>();
        var controller = new MenuController(mockMenuUi.Object);
        var httpContext = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

        var model = new MenuItemFormViewModel
        {
            ItemCode = "PBM-1",
            NameEn = "Paneer Butter Masala",
            NameMr = "पनीर बटर मसाला",
            CategoryId = "Main Course",
            Price = 280m,
            TaxRate = 5m,
            PrepTimeMinutes = 15
        };

        // Save and New
        var saveNewResult = controller.ItemForm(model, "saveAndNew").Should().BeOfType<RedirectToActionResult>().Subject;
        saveNewResult.ActionName.Should().Be("ItemForm");
        controller.TempData["HighlightedRow"].Should().Be("Paneer Butter Masala");

        // Regular Save
        var saveResult = controller.ItemForm(model, "save").Should().BeOfType<RedirectToActionResult>().Subject;
        saveResult.ControllerName.Should().Be("Masters");
        saveResult.ActionName.Should().Be("Index");
        saveResult.RouteValues!["tab"].Should().Be("menu-items");
    }

    [Fact]
    public void MenuController_QuickAddCategory_ReturnsResultJsonShape()
    {
        var controller = new MenuController(Mock.Of<IMenuUiData>());

        var failJson = controller.QuickAddCategory("", null).Should().BeOfType<JsonResult>().Subject;
        System.Text.Json.JsonSerializer.Serialize(failJson.Value).Should().Contain("\"isSuccess\":false");

        var okJson = controller.QuickAddCategory("Tandoor Specials", "तंदूर स्पेशल").Should().BeOfType<JsonResult>().Subject;
        var okStr = System.Text.Json.JsonSerializer.Serialize(okJson.Value);
        okStr.Should().Contain("\"isSuccess\":true");
        okStr.Should().Contain("Tandoor Specials");
    }
}

using FluentAssertions;
using RestaurantMS.Web.Models;
using RestaurantMS.Web.Validators;
using Xunit;

namespace RestaurantMS.Integration.Tests;

public class UiValidatorTests
{
    [Fact]
    public void CustomerFormViewModelValidator_ValidCustomer_Passes()
    {
        var validator = new CustomerFormViewModelValidator();
        var model = new CustomerFormViewModel
        {
            FullName = "Ramesh Patil",
            MobileNumber = "9822112233",
            CreditLimit = 5000m,
            IsCreditAllowed = true
        };

        var result = validator.Validate(model);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CustomerFormViewModelValidator_InvalidMobile_Fails()
    {
        var validator = new CustomerFormViewModelValidator();
        var model = new CustomerFormViewModel
        {
            FullName = "Ramesh Patil",
            MobileNumber = "12345",
            CreditLimit = 5000m
        };

        var result = validator.Validate(model);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "MobileNumber");
    }

    [Fact]
    public void CategoryFormViewModelValidator_Valid_Passes()
    {
        var validator = new CategoryFormViewModelValidator();
        var model = new CategoryFormViewModel
        {
            NameEn = "Main Course",
            NameMr = "मुख्य जेवण",
            DisplayOrder = 1
        };

        var result = validator.Validate(model);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void MenuItemFormViewModelValidator_ZeroPrice_Fails()
    {
        var validator = new MenuItemFormViewModelValidator();
        var model = new MenuItemFormViewModel
        {
            NameEn = "Paneer Tikka",
            CategoryId = "1",
            Price = 0m,
            TaxRate = 5m
        };

        var result = validator.Validate(model);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }

    [Fact]
    public void SupplierFormViewModelValidator_InvalidPhone_Fails()
    {
        var validator = new SupplierFormViewModelValidator();
        var model = new SupplierFormViewModel
        {
            Name = "Amul Dairy",
            ContactPerson = "Mr. Patel",
            Phone = "0000"
        };

        var result = validator.Validate(model);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Phone");
    }

    [Fact]
    public void ReservationFormViewModelValidator_Valid_Passes()
    {
        var validator = new ReservationFormViewModelValidator();
        var model = new ReservationFormViewModel
        {
            CustomerName = "Sachin Tendulkar",
            MobileNumber = "9822334455",
            GuestCount = 4,
            TableNumber = "T5"
        };

        var result = validator.Validate(model);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void StaffFormViewModelValidator_EmptyName_Fails()
    {
        var validator = new StaffFormViewModelValidator();
        var model = new StaffFormViewModel
        {
            FullName = "",
            Role = "Waiter",
            MobileNumber = "9822112233",
            Shift = "Morning"
        };

        var result = validator.Validate(model);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FullName");
    }

    [Fact]
    public void CreateParcelViewModelValidator_Valid_Passes()
    {
        var validator = new CreateParcelViewModelValidator();
        var model = new CreateParcelViewModel
        {
            CustomerName = "Anil Kumble",
            MobileNumber = "9822998877",
            PaymentMode = "Cash"
        };

        var result = validator.Validate(model);
        result.IsValid.Should().BeTrue();
    }
}

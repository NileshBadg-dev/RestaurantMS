namespace RestaurantMS.Application.Common.Models.UiData;

public record MasterCustomerDto(
    long Id,
    string Name,
    string Mobile,
    string Address,
    decimal CreditLimit,
    decimal OutstandingBalance,
    string Status);

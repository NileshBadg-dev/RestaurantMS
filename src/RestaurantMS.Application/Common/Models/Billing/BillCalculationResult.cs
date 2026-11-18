namespace RestaurantMS.Application.Common.Models.Billing;

public record BillCalculationResult(
    decimal Subtotal,
    decimal DiscountAmount,
    decimal PackingCharge,
    decimal TaxableAmount,
    decimal GstPercent,
    decimal CgstAmount,
    decimal SgstAmount,
    decimal TotalTax,
    decimal RawTotal,
    decimal RoundOff,
    decimal GrandTotal);

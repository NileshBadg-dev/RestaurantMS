namespace RestaurantMS.Application.Common.Models.Inventory;

public record StockLedgerEntryDto(
    long Id,
    DateTime EntryDate,
    long StockItemId,
    string ItemName,
    string Unit,
    string MovementType,
    string ReferenceNo,
    decimal InQty,
    decimal OutQty,
    decimal BalanceQty,
    decimal Rate,
    decimal ValueImpact,
    string? Reason,
    string Actor);

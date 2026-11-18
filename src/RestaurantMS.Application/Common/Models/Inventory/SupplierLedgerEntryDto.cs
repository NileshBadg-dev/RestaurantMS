namespace RestaurantMS.Application.Common.Models.Inventory;

public record SupplierLedgerEntryDto(
    long Id,
    long SupplierId,
    string SupplierName,
    DateTime EntryDate,
    string EntryType,
    string ReferenceNo,
    decimal Debit,
    decimal Credit,
    decimal RunningBalance,
    string? Notes);

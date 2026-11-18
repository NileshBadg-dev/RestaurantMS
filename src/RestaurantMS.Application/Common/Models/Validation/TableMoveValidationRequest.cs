namespace RestaurantMS.Application.Common.Models.Validation;

public record TableMoveValidationRequest(
    string SourceTableCode,
    string TargetTableCode,
    bool TargetTableOccupied,
    int ActiveKotCount,
    bool ConfirmKotTransfer);

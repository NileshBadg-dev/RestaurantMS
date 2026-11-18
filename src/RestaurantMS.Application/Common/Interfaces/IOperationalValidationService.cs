using RestaurantMS.Application.Common.Models.Validation;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Common.Interfaces;

/// <summary>
/// Centralized operational and domain integrity guards: duplicates, delete-in-use, concurrency, locked periods, table occupancy and overrides.
/// </summary>
public interface IOperationalValidationService
{
    Result CheckDuplicate(string entityLabel, string fieldLabel, string candidate, IEnumerable<string> existingValues);

    Result CheckCanDelete(int referencedBillCount);

    Result CheckConcurrency(byte[]? expectedRowVersion, byte[]? currentRowVersion);

    Result CheckTableCanOccupy(string tableCode, bool isCurrentlyOccupied);

    Result CheckTableMove(TableMoveValidationRequest request);

    Result CheckAttendanceMonthUnlocked(int year, int month, bool isMonthLocked);

    Task<Result> CheckAccountingDateUnlockedAsync(DateTime date, CancellationToken cancellationToken = default);

    Result CheckPermissionAndOverride(
        string requiredPermission,
        IReadOnlyCollection<string> userPermissions,
        bool isSuperUser,
        bool limitExceeded,
        bool isManagerOverride,
        string? managerOverrideReason);
}

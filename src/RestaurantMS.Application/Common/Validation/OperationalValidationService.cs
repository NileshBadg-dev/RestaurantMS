using System.Globalization;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Models.Validation;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Common.Validation;

public class OperationalValidationService : IOperationalValidationService
{
    private readonly IAccountingService _accountingService;

    public OperationalValidationService(IAccountingService accountingService)
    {
        _accountingService = accountingService;
    }

    public Result CheckDuplicate(string entityLabel, string fieldLabel, string candidate, IEnumerable<string> existingValues)
    {
        var value = (candidate ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(value))
        {
            return Result.Success();
        }

        var exists = existingValues.Any(v => string.Equals(v?.Trim(), value, StringComparison.OrdinalIgnoreCase));
        if (exists)
        {
            return Result.Failure(
                "Validation.Duplicate",
                $"A {entityLabel} with {fieldLabel} '{value}' already exists. Enter a unique {fieldLabel}.");
        }

        return Result.Success();
    }

    public Result CheckCanDelete(int referencedBillCount)
    {
        if (referencedBillCount > 0)
        {
            return Result.Failure(
                "Validation.RecordInUse",
                $"Cannot delete, used in {referencedBillCount} bills. Deactivate instead");
        }

        return Result.Success();
    }

    public Result CheckConcurrency(byte[]? expectedRowVersion, byte[]? currentRowVersion)
    {
        if (expectedRowVersion is null || currentRowVersion is null)
        {
            return Result.Success();
        }

        if (!expectedRowVersion.SequenceEqual(currentRowVersion))
        {
            return Result.Failure(
                "Validation.ConcurrencyConflict",
                "This record was updated by another user while you were editing. Reload the latest data and apply your changes again.");
        }

        return Result.Success();
    }

    public Result CheckTableCanOccupy(string tableCode, bool isCurrentlyOccupied)
    {
        if (isCurrentlyOccupied)
        {
            return Result.Failure(
                "Tables.AlreadyOccupied",
                $"Table {tableCode} is already occupied. Open the running order or select a free table.");
        }

        return Result.Success();
    }

    public Result CheckTableMove(TableMoveValidationRequest request)
    {
        var result = new TableMoveValidationRequestValidator().Validate(request);
        return result.IsValid
            ? Result.Success()
            : Result.Failure("Tables.MoveInvalid", result.Errors[0].ErrorMessage);
    }

    public Result CheckAttendanceMonthUnlocked(int year, int month, bool isMonthLocked)
    {
        if (isMonthLocked)
        {
            var monthName = new DateTime(year, Math.Clamp(month, 1, 12), 1).ToString("MMMM yyyy", CultureInfo.InvariantCulture);
            return Result.Failure(
                "Staff.AttendanceLocked",
                $"Attendance for {monthName} is locked after payroll finalization and cannot be edited.");
        }

        return Result.Success();
    }

    public async Task<Result> CheckAccountingDateUnlockedAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var locked = await _accountingService.IsPeriodLockedAsync(date, cancellationToken);
        if (locked)
        {
            var lockDate = await _accountingService.GetLockDateAsync(cancellationToken);
            var lockLabel = lockDate.HasValue ? lockDate.Value.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) : "the closed period";
            return Result.Failure(
                "Accounting.PeriodLocked",
                $"Transactions on {date:dd-MMM-yyyy} are blocked because accounting is locked up to {lockLabel}.");
        }

        return Result.Success();
    }

    public Result CheckPermissionAndOverride(
        string requiredPermission,
        IReadOnlyCollection<string> userPermissions,
        bool isSuperUser,
        bool limitExceeded,
        bool isManagerOverride,
        string? managerOverrideReason)
    {
        var hasPerm = isSuperUser || userPermissions.Contains(requiredPermission, StringComparer.OrdinalIgnoreCase);
        if (!hasPerm)
        {
            return Result.Failure(
                "Auth.Forbidden",
                $"You do not have permission ({requiredPermission}) to perform this action.");
        }

        if (limitExceeded)
        {
            if (!isManagerOverride)
            {
                return Result.Failure(
                    "Validation.OverrideRequired",
                    "Configured limit exceeded. Manager override with a written reason is required.");
            }

            if (string.IsNullOrWhiteSpace(managerOverrideReason))
            {
                return Result.Failure(
                    "Validation.OverrideReasonRequired",
                    "Enter a clear manager override reason before proceeding.");
            }
        }

        return Result.Success();
    }
}

using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Models.Accounting;
using RestaurantMS.Application.Common.Models.Billing;
using RestaurantMS.Application.Common.Models.Validation;
using RestaurantMS.Application.Common.Validation;
using RestaurantMS.Application.Settings.Commands.SaveSettings;
using RestaurantMS.Domain.Constants;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Tests;

public class ValidationAuditSuiteTests
{
    private static readonly DateTime FixedUtc = new(2026, 11, 18, 6, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime TodayIst = new(2026, 11, 18);

    [Theory]
    [InlineData("9876543210", true)]
    [InlineData("6123456789", true)]
    [InlineData("5123456789", false)]
    [InlineData("987654321", false)]
    [InlineData("abcdefghij", false)]
    public void IndianValidationRules_Mobile_ValidatesExactIndianFormat(string mobile, bool expected)
    {
        Assert.Equal(expected, IndianValidationRules.IsValidMobile(mobile));
    }

    [Theory]
    [InlineData("ABCDE1234F", true)]
    [InlineData("abcde1234f", true)]
    [InlineData("ABCD12345F", false)]
    [InlineData("ABCDE1234", false)]
    public void IndianValidationRules_Pan_ValidatesTenCharFormat(string pan, bool expected)
    {
        Assert.Equal(expected, IndianValidationRules.IsValidPan(pan));
    }

    [Theory]
    [InlineData("grandrestaurant@okhdfc", true)]
    [InlineData("hotel.pune-1@ybl", true)]
    [InlineData("invalidupi", false)]
    [InlineData("@okhdfc", false)]
    [InlineData("user@123", false)]
    public void IndianValidationRules_UpiId_ValidatesVpaFormat(string upi, bool expected)
    {
        Assert.Equal(expected, IndianValidationRules.IsValidUpiId(upi));
    }

    [Fact]
    public void IndianValidationRules_GstinAndDecimals_ValidateChecksumAndPrecision()
    {
        Assert.True(IndianValidationRules.IsValidGstin("27AAPFU0939F1ZV"));
        Assert.False(IndianValidationRules.IsValidGstin("27AAPFU0939F1ZA")); // wrong checksum
        Assert.True(IndianValidationRules.HasMaxDecimals(125.50m, 2));
        Assert.False(IndianValidationRules.HasMaxDecimals(125.555m, 2));
    }

    [Fact]
    public void SaveSettingsCommandValidator_RejectsInvalidUpiIdWithHelpfulMessage()
    {
        var validator = new SaveSettingsCommandValidator();
        var cmd = new SaveSettingsCommand("Grand Hotel", "भव्य हॉटेल", HotelUpiId: "not-a-upi");

        var result = validator.Validate(cmd);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "HotelUpiId" && e.ErrorMessage.Contains("UPI ID"));
    }

    [Fact]
    public void MenuItemValidator_ChecksRequiredLimitsAndDecimals()
    {
        var validator = new MenuItemValidationRequestValidator();

        var invalid = validator.Validate(new MenuItemValidationRequest("!", "", "", 0, 12.345m, 35m, 0));
        Assert.False(invalid.IsValid);
        Assert.True(invalid.Errors.Count >= 5);
        Assert.DoesNotContain(invalid.Errors, e => e.ErrorMessage.Equals("Invalid input", StringComparison.OrdinalIgnoreCase));

        var valid = validator.Validate(new MenuItemValidationRequest("M01", "Paneer Tikka", "पनीर टिक्का", 1, 240.50m, 5m, 15));
        Assert.True(valid.IsValid);
    }

    [Fact]
    public void CustomerAndSupplierValidators_ValidateMobileEmailGstinPanUpiAndCreditAddress()
    {
        var custValidator = new CustomerValidationRequestValidator();
        var badCust = custValidator.Validate(new CustomerValidationRequest("Ramesh", "12345", "bad-email", "", "27BADGSTIN00000", "BADPAN", 5000.555m, true));
        Assert.False(badCust.IsValid);
        Assert.Contains(badCust.Errors, e => e.PropertyName == "Mobile");
        Assert.Contains(badCust.Errors, e => e.PropertyName == "Address");
        Assert.Contains(badCust.Errors, e => e.PropertyName == "Pan");

        var goodCust = custValidator.Validate(new CustomerValidationRequest("Ramesh Patil", "9876543210", "ramesh@example.com", "14 Shivaji Nagar, Pune", "27AAPFU0939F1ZV", "ABCDE1234F", 10000m, true));
        Assert.True(goodCust.IsValid);

        var supValidator = new SupplierValidationRequestValidator();
        var badSup = supValidator.Validate(new SupplierValidationRequest("A", "", "123", "bad", "BAD", "BAD", "badupi"));
        Assert.False(badSup.IsValid);

        var goodSup = supValidator.Validate(new SupplierValidationRequest("Sahyadri Dairy", "Santosh", "9823022334", "sales@sahyadri.in", "27AAPFU0939F1ZV", "ABCDE1234F", "sahyadri@okhdfc"));
        Assert.True(goodSup.IsValid);
    }

    [Fact]
    public void StaffReservationDayClosingVoucherAndBillValidators_EnforceDomainRules()
    {
        var clock = new FixedDateTime(FixedUtc);

        // Staff joining date cannot be future & salary > 0
        var staffVal = new StaffProfileValidationRequestValidator(clock);
        Assert.False(staffVal.Validate(new StaffProfileValidationRequest("EMP-1", "Suresh", "9876543210", null, TodayIst.AddDays(2), 0m, "INVALID", "badupi")).IsValid);
        Assert.True(staffVal.Validate(new StaffProfileValidationRequest("EMP-1", "Suresh Patil", "9876543210", "s@h.com", TodayIst, 18000m, "ABCDE1234F", "suresh@ybl")).IsValid);

        // Reservation cannot be in the past
        var resVal = new ReservationValidationRequestValidator(clock);
        Assert.False(resVal.Validate(new ReservationValidationRequest("Amit", "9876543210", 4, TodayIst.AddDays(-1), "20:00")).IsValid);
        Assert.True(resVal.Validate(new ReservationValidationRequest("Amit Deshmukh", "9876543210", 4, TodayIst, "20:00")).IsValid);

        // Day closing requires remarks on cash variance & blocks future date
        var dcVal = new DayClosingValidationRequestValidator(clock);
        Assert.False(dcVal.Validate(new DayClosingValidationRequest(TodayIst, 5000m, 40000m, 39500m, "")).IsValid);
        Assert.True(dcVal.Validate(new DayClosingValidationRequest(TodayIst, 5000m, 40000m, 39500m, "Petty cash paid for ice")).IsValid);

        // Voucher must balance (Dr == Cr) & have >= 2 lines
        var vchVal = new PostVoucherRequestValidator();
        Assert.False(vchVal.Validate(new PostVoucherRequest(TodayIst, "Journal", "Test", "REF", [new("1110", "Cash", 500m, 0m), new("4100", "Sales", 0m, 400m)])).IsValid);
        Assert.True(vchVal.Validate(new PostVoucherRequest(TodayIst, "Journal", "Test", "REF", [new("1110", "Cash", 500m, 0m), new("4100", "Sales", 0m, 500m)])).IsValid);

        // Bill settlement rejects empty items & override without reason
        var billVal = new SettleBillRequestValidator();
        Assert.False(billVal.Validate(new SettleBillRequest("k1", "Dine-in", "T1", null, "Waiter", [], 0m, 0m, 5m, "Cash", [], IsManagerOverride: true, ManagerOverrideReason: "")).IsValid);
    }

    [Fact]
    public async Task OperationalValidationService_CoversDuplicatesDeleteInUseConcurrencyLocksTableMoveAndOverrides()
    {
        var accounting = new LockedAccountingService(new DateTime(2026, 9, 30));
        var sut = new OperationalValidationService(accounting);

        // 1. Duplicates (name, code, mobile)
        var dupMobile = sut.CheckDuplicate("customer", "mobile number", "9876543210", ["9876543210", "9822012345"]);
        Assert.True(dupMobile.IsFailure);
        Assert.Contains("already exists", dupMobile.Error.Message);

        var dupCode = sut.CheckDuplicate("menu item", "short code", "M01", ["m01", "M02"]);
        Assert.True(dupCode.IsFailure);

        // 2. Delete of used records — exact prompt wording
        var delBlocked = sut.CheckCanDelete(7);
        Assert.True(delBlocked.IsFailure);
        Assert.Equal("Cannot delete, used in 7 bills. Deactivate instead", delBlocked.Error.Message);
        Assert.True(sut.CheckCanDelete(0).IsSuccess);

        // 3. Concurrency conflict (RowVersion)
        var conflict = sut.CheckConcurrency([1, 2, 3, 4], [1, 2, 3, 5]);
        Assert.True(conflict.IsFailure);
        Assert.Equal("Validation.ConcurrencyConflict", conflict.Error.Code);
        Assert.True(sut.CheckConcurrency([1, 2, 3, 4], [1, 2, 3, 4]).IsSuccess);

        // 4. Table already occupied
        var occupied = sut.CheckTableCanOccupy("T2", true);
        Assert.True(occupied.IsFailure);
        Assert.Contains("already occupied", occupied.Error.Message);

        // 5. Moving a table with a running KOT & into occupied target
        var moveOccupied = sut.CheckTableMove(new TableMoveValidationRequest("T2", "T4", TargetTableOccupied: true, ActiveKotCount: 1, ConfirmKotTransfer: true));
        Assert.True(moveOccupied.IsFailure);
        Assert.Contains("already occupied", moveOccupied.Error.Message);

        var moveUnconfirmedKot = sut.CheckTableMove(new TableMoveValidationRequest("T2", "T6", TargetTableOccupied: false, ActiveKotCount: 2, ConfirmKotTransfer: false));
        Assert.True(moveUnconfirmedKot.IsFailure);
        Assert.Contains("2 active KOT(s)", moveUnconfirmedKot.Error.Message);

        var moveConfirmedKot = sut.CheckTableMove(new TableMoveValidationRequest("T2", "T6", TargetTableOccupied: false, ActiveKotCount: 2, ConfirmKotTransfer: true));
        Assert.True(moveConfirmedKot.IsSuccess);

        // 6. Attendance on locked month
        var lockedAtt = sut.CheckAttendanceMonthUnlocked(2026, 10, isMonthLocked: true);
        Assert.True(lockedAtt.IsFailure);
        Assert.Contains("October 2026", lockedAtt.Error.Message);
        Assert.Contains("locked", lockedAtt.Error.Message, StringComparison.OrdinalIgnoreCase);

        // 7. Locked accounting period
        var lockedAcct = await sut.CheckAccountingDateUnlockedAsync(new DateTime(2026, 9, 15));
        Assert.True(lockedAcct.IsFailure);
        Assert.Equal("Accounting.PeriodLocked", lockedAcct.Error.Code);
        Assert.True((await sut.CheckAccountingDateUnlockedAsync(new DateTime(2026, 11, 18))).IsSuccess);

        // 8. Permissions & Manager Override with reason
        var noPerm = sut.CheckPermissionAndOverride(Permissions.Billing.Edit, [Permissions.Billing.View], false, false, false, null);
        Assert.True(noPerm.IsFailure);
        Assert.Equal("Auth.Forbidden", noPerm.Error.Code);

        var overrideMissingReason = sut.CheckPermissionAndOverride(Permissions.Billing.Edit, [Permissions.Billing.Edit], false, true, true, "");
        Assert.True(overrideMissingReason.IsFailure);
        Assert.Equal("Validation.OverrideReasonRequired", overrideMissingReason.Error.Code);

        var overrideValid = sut.CheckPermissionAndOverride(Permissions.Billing.Edit, [Permissions.Billing.Edit], false, true, true, "Approved by restaurant manager");
        Assert.True(overrideValid.IsSuccess);
    }

    private sealed class LockedAccountingService(DateTime lockDate) : IAccountingService
    {
        public Task<bool> IsPeriodLockedAsync(DateTime date, CancellationToken cancellationToken = default) =>
            Task.FromResult(date.Date <= lockDate.Date);
        public Task<DateTime?> GetLockDateAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<DateTime?>(lockDate);
        public Task SetLockDateAsync(DateTime? d, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<Result<PostedVoucherDto>> PostVoucherAsync(PostVoucherRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result.Failure<PostedVoucherDto>("Accounting.PeriodLocked", "Locked"));
        public Task<IReadOnlyList<PostedVoucherDto>> GetPostedVouchersAsync(DateTime? fromDate = null, DateTime? toDate = null, string? voucherType = null, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<PostedVoucherDto>>([]);
    }

    private sealed class FixedDateTime(DateTime utcNow) : IDateTime
    {
        public DateTime UtcNow { get; } = utcNow;
    }
}

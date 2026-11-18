using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Models.Accounting;
using RestaurantMS.Application.Common.Models.Staff;
using RestaurantMS.Application.Staff.Services;
using RestaurantMS.Application.Staff.Validators;
using RestaurantMS.Infrastructure.Services;
using RestaurantMS.Web.Controllers;
using RestaurantMS.Web.SampleData;
using Xunit;

namespace RestaurantMS.Integration.Tests;

public class EmployeeAdvancesAndStaffTests
{
    private static (
        InMemoryAccountingService Accounting,
        SampleEmployeeAdvancesUiData Advances,
        SampleStaffUiData Staff,
        SampleAccountsUiData Accounts) CreateServices(string maxPercent = "100")
    {
        var mockDate = new Mock<IDateTime>();
        mockDate.Setup(d => d.UtcNow).Returns(new DateTime(2026, 11, 18, 10, 0, 0, DateTimeKind.Utc));

        var mockSettings = new Mock<ISettingService>();
        mockSettings.Setup(s => s.GetSettingAsync("MaxAdvanceSalaryPercent", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(maxPercent);

        var accounting = new InMemoryAccountingService(mockDate.Object);
        var staffStore = new SampleStaffStore();
        var advState = new SampleAdvanceStateStore();
        var attStore = new SampleAttendanceSalaryStore();
        var salaryCalc = new SalaryCalculationService();

        var advances = new SampleEmployeeAdvancesUiData(
            staffStore,
            advState,
            accounting,
            mockSettings.Object,
            mockDate.Object,
            new GiveEmployeeAdvanceRequestValidator(),
            new RecordAdvanceRecoveryRequestValidator());

        var staff = new SampleStaffUiData(staffStore, attStore, advances, salaryCalc, mockDate.Object);
        var accounts = new SampleAccountsUiData(accounting);

        return (accounting, advances, staff, accounts);
    }

    [Fact]
    public async Task AccountingService_UnbalancedVoucher_IsRejected()
    {
        var (accounting, _, _, _) = CreateServices();

        var unbalanced = new PostVoucherRequest(
            new DateTime(2026, 11, 18),
            "Payment",
            "Broken voucher",
            "ERR-1",
            [
                new AccountingVoucherLineDto("1150", "Employee Advances A/c", 1000m, 0m),
                new AccountingVoucherLineDto("1110", "Cash in Hand", 0m, 800m)
            ]);

        var result = await accounting.PostVoucherAsync(unbalanced);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Accounting.UnbalancedVoucher");
    }

    [Fact]
    public async Task GiveAdvance_PostsBalancedVoucherAndAppearsInDayBook()
    {
        var (accounting, advances, _, accounts) = CreateServices();
        var date = new DateTime(2026, 11, 18);

        var result = await advances.GiveAdvanceAsync(new GiveEmployeeAdvanceRequest
        {
            StaffId = 3, // Meena Jadhav (Active, Salary 24,000, Outstanding 0)
            Date = date,
            Amount = 6000m,
            PaymentMode = "Bank",
            Purpose = "Home Repair",
            RecoveryPlan = "MonthlyInstalment",
            MonthlyInstalmentAmount = 2000m,
            StartMonth = "2026-12"
        });

        result.IsSuccess.Should().BeTrue();
        result.Value.RunningBalance.Should().Be(6000m);

        var vouchers = await accounting.GetPostedVouchersAsync(date, date);
        vouchers.Should().ContainSingle(v => v.VoucherNumber == result.Value.VoucherNumber);
        var posted = vouchers.Single(v => v.VoucherNumber == result.Value.VoucherNumber);
        posted.TotalDebit.Should().Be(6000m);
        posted.TotalCredit.Should().Be(6000m);
        posted.Lines.Should().Contain(l => l.AccountCode == "1150" && l.Debit == 6000m);
        posted.Lines.Should().Contain(l => l.AccountCode == "1120" && l.Credit == 6000m);

        var dayBook = await accounts.GetDayBookAsync(date);
        dayBook.Should().Contain(e => e.VoucherNo == result.Value.VoucherNumber && e.DebitAmount == 6000m);
        dayBook.Should().Contain(e => e.VoucherNo == result.Value.VoucherNumber && e.CreditAmount == 6000m);
    }

    [Fact]
    public async Task GiveAdvance_Validations_EnforceInactiveLockedPeriodAndSalaryCap()
    {
        var (_, advances, _, _) = CreateServices(maxPercent: "100");

        // 1. Inactive staff (StaffId 7 = Ganesh More, Active=false)
        var inactiveResult = await advances.GiveAdvanceAsync(new GiveEmployeeAdvanceRequest
        {
            StaffId = 7,
            Date = new DateTime(2026, 11, 18),
            Amount = 1000m,
            PaymentMode = "Cash",
            Purpose = "Test",
            RecoveryPlan = "OneTime"
        });
        inactiveResult.IsFailure.Should().BeTrue();
        inactiveResult.Error.Code.Should().Be("Advance.InactiveStaff");

        // 2. Locked period (<= 2026-09-30)
        var lockedResult = await advances.GiveAdvanceAsync(new GiveEmployeeAdvanceRequest
        {
            StaffId = 3,
            Date = new DateTime(2026, 9, 25),
            Amount = 1000m,
            PaymentMode = "Cash",
            Purpose = "Old Date",
            RecoveryPlan = "OneTime"
        });
        lockedResult.IsFailure.Should().BeTrue();
        lockedResult.Error.Code.Should().Be("Advance.PeriodLocked");

        // 3. Exceeds 100% salary without override (Meena salary 24,000)
        var overCapResult = await advances.GiveAdvanceAsync(new GiveEmployeeAdvanceRequest
        {
            StaffId = 3,
            Date = new DateTime(2026, 11, 18),
            Amount = 30000m,
            PaymentMode = "Bank",
            Purpose = "Major Medical",
            RecoveryPlan = "OneTime",
            IsManagerOverride = false
        });
        overCapResult.IsFailure.Should().BeTrue();
        overCapResult.Error.Code.Should().Be("Advance.ExceedsSalaryLimit");

        // 4. Succeeds WITH manager override and reason
        var overrideResult = await advances.GiveAdvanceAsync(new GiveEmployeeAdvanceRequest
        {
            StaffId = 3,
            Date = new DateTime(2026, 11, 18),
            Amount = 30000m,
            PaymentMode = "Bank",
            Purpose = "Major Medical",
            RecoveryPlan = "OneTime",
            IsManagerOverride = true,
            ManagerOverrideReason = "Approved by Owner for hospitalization"
        });
        overrideResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task RecordRecovery_ReversesBalancedVoucherAndRejectsExcessRecovery()
    {
        var (accounting, advances, _, _) = CreateServices();

        // StaffId 1 (Suresh Patil) starts with Outstanding = 2,500
        var excessResult = await advances.RecordRecoveryAsync(new RecordAdvanceRecoveryRequest
        {
            StaffId = 1,
            Date = new DateTime(2026, 11, 18),
            Amount = 3000m,
            PaymentMode = "Cash"
        });
        excessResult.IsFailure.Should().BeTrue();
        excessResult.Error.Code.Should().Be("Advance.RecoveryExceedsOutstanding");

        var okResult = await advances.RecordRecoveryAsync(new RecordAdvanceRecoveryRequest
        {
            StaffId = 1,
            Date = new DateTime(2026, 11, 18),
            Amount = 1500m,
            PaymentMode = "Cash",
            Note = "Partial cash return"
        });
        okResult.IsSuccess.Should().BeTrue();
        okResult.Value.RunningBalance.Should().Be(1000m);

        var vouchers = await accounting.GetPostedVouchersAsync();
        var posted = vouchers.Single(v => v.VoucherNumber == okResult.Value.VoucherNumber);
        posted.TotalDebit.Should().Be(1500m);
        posted.TotalCredit.Should().Be(1500m);
        posted.Lines.Should().Contain(l => l.AccountCode == "1110" && l.Debit == 1500m);
        posted.Lines.Should().Contain(l => l.AccountCode == "1150" && l.Credit == 1500m);
    }

    [Fact]
    public async Task StaffModule_MonthLockBlocksAttendanceAndSalaryPaidPostsRecovery()
    {
        var (_, advances, staff, _) = CreateServices();

        // Lock Nov 2026 and verify attendance edits fail
        await staff.SetMonthLockAsync(11, 2026, true);
        var lockedEdit = await staff.SaveAttendanceEntryAsync(1, new DateTime(2026, 11, 18), "Absent", "-", "-", 0m, "Test");
        lockedEdit.IsFailure.Should().BeTrue();
        lockedEdit.Error.Code.Should().Be("Attendance.MonthLocked");

        // Mark salary paid for StaffId 1 (Outstanding 2,500, Planned instalment 2,500)
        var paidResult = await staff.MarkSalaryPaidAsync(1, 11, 2026, autoRecoverAdvance: true);
        paidResult.IsSuccess.Should().BeTrue();
        paidResult.Value.Status.Should().Be("Paid");
        paidResult.Value.RecoveryVoucherNumber.Should().NotBeNullOrWhiteSpace();

        var summaryAfter = await advances.GetEmployeeSummaryAsync(1);
        summaryAfter!.OutstandingBalance.Should().Be(0m);
    }

    [Fact]
    public void StaffController_AdvancesRoute_RedirectsPermanentToEmployeeAdvances()
    {
        var (_, advances, staff, _) = CreateServices();
        var mockSettings = new Mock<ISettingService>();
        var mockDate = new Mock<IDateTime>();
        var controller = new StaffController(staff, advances, mockSettings.Object, mockDate.Object);

        var redirect = controller.Advances().Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.Permanent.Should().BeTrue();
        redirect.ControllerName.Should().Be("EmployeeAdvances");
        redirect.ActionName.Should().Be("Index");
    }
}

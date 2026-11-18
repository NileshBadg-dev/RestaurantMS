using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Application.Common.Models.Staff;
using RestaurantMS.Domain.Constants;
using RestaurantMS.Web.Models;

namespace RestaurantMS.Web.Controllers;

[Authorize]
[Route("Accounts/EmployeeAdvances")]
public class EmployeeAdvancesController : Controller
{
    private readonly IEmployeeAdvancesUiData _advancesUiData;
    private readonly IAccountingService _accountingService;
    private readonly ISalaryCalculationService _salaryCalculator;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTime _dateTime;

    public EmployeeAdvancesController(
        IEmployeeAdvancesUiData advancesUiData,
        IAccountingService accountingService,
        ISalaryCalculationService salaryCalculator,
        ICurrentUser currentUser,
        IDateTime dateTime)
    {
        _advancesUiData = advancesUiData;
        _accountingService = accountingService;
        _salaryCalculator = salaryCalculator;
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(long? staffId = null, string? search = null, CancellationToken cancellationToken = default)
    {
        var kpis = await _advancesUiData.GetKpisAsync(cancellationToken);
        var employees = await _advancesUiData.GetEmployeeSummariesAsync(search, cancellationToken);
        var selectedId = staffId ?? employees.FirstOrDefault()?.StaffId ?? 0;
        var selectedEmployee = employees.FirstOrDefault(e => e.StaffId == selectedId)
                               ?? (selectedId > 0 ? await _advancesUiData.GetEmployeeSummaryAsync(selectedId, cancellationToken) : null);

        var ledger = selectedEmployee is not null
            ? await _advancesUiData.GetEmployeeLedgerAsync(selectedEmployee.StaffId, cancellationToken)
            : [];

        var maxPercent = await _advancesUiData.GetMaxAdvancePercentAsync(cancellationToken);
        var lockDate = await _accountingService.GetLockDateAsync(cancellationToken);

        var viewModel = new EmployeeAdvancesViewModel
        {
            Kpis = kpis,
            Employees = employees,
            SelectedStaffId = selectedEmployee?.StaffId ?? 0,
            SelectedEmployee = selectedEmployee,
            LedgerEntries = ledger,
            Search = search,
            MaxAdvanceSalaryPercent = maxPercent,
            AccountingLockDate = lockDate,
            CanGiveAdvance = HasPermission(Permissions.EmployeeAdvances.Create),
            CanRecoverAdvance = HasPermission(Permissions.EmployeeAdvances.Recover),
            CanApproveOverride = HasPermission(Permissions.EmployeeAdvances.ApproveOverride)
        };

        return View(viewModel);
    }

    [HttpGet("PreviewSchedule")]
    public IActionResult PreviewSchedule(decimal amount, string recoveryPlan, decimal? monthlyInstalmentAmount, string? startMonth)
    {
        var schedule = _salaryCalculator.BuildInstalmentSchedule(amount, recoveryPlan, monthlyInstalmentAmount, startMonth);
        return Json(new { isSuccess = true, value = schedule });
    }

    [HttpPost("Give")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GiveAdvance(GiveEmployeeAdvanceRequest request, CancellationToken cancellationToken)
    {
        if (request.IsManagerOverride && !HasPermission(Permissions.EmployeeAdvances.ApproveOverride))
        {
            return HandleMutationFailure(request.StaffId, "You do not have permission to approve a manager salary-cap override.");
        }

        var result = await _advancesUiData.GiveAdvanceAsync(request, cancellationToken);
        if (result.IsFailure)
        {
            return HandleMutationFailure(request.StaffId, result.Error.Description);
        }

        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
        {
            return Json(new { isSuccess = true, value = result.Value });
        }

        TempData["SuccessMessage"] = $"Advance of ₹{result.Value.Amount:N2} posted for {result.Value.EmployeeName} (Voucher {result.Value.VoucherNumber}).";
        return RedirectToAction(nameof(Index), new { staffId = request.StaffId });
    }

    [HttpPost("Recover")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordRecovery(RecordAdvanceRecoveryRequest request, CancellationToken cancellationToken)
    {
        var result = await _advancesUiData.RecordRecoveryAsync(request, cancellationToken);
        if (result.IsFailure)
        {
            return HandleMutationFailure(request.StaffId, result.Error.Description);
        }

        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
        {
            return Json(new { isSuccess = true, value = result.Value });
        }

        TempData["SuccessMessage"] = $"Recovery of ₹{result.Value.Amount:N2} posted for {result.Value.EmployeeName} (Voucher {result.Value.VoucherNumber}).";
        return RedirectToAction(nameof(Index), new { staffId = request.StaffId });
    }

    [HttpPost("ToggleAutoRecover")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAutoRecover(long staffId, bool enabled, CancellationToken cancellationToken)
    {
        var result = await _advancesUiData.SetAutoRecoverAsync(staffId, enabled, cancellationToken);
        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
        {
            return Json(new { isSuccess = result.IsSuccess, value = result.Value });
        }

        TempData["SuccessMessage"] = enabled
            ? "Auto-recovery from Salary Statement enabled."
            : "Auto-recovery from Salary Statement paused.";
        return RedirectToAction(nameof(Index), new { staffId });
    }

    [HttpGet("Reports")]
    public async Task<IActionResult> Reports(string? report = null, long? staffId = null, int? month = null, int? year = null, CancellationToken cancellationToken = default)
    {
        var now = _dateTime.UtcNow;
        var selectedMonth = month ?? now.Month;
        var selectedYear = year ?? now.Year;
        var reportType = (report ?? "outstanding").Trim().ToLowerInvariant();
        if (reportType is not ("ledger" or "outstanding" or "recovery"))
        {
            reportType = "outstanding";
        }

        var employees = await _advancesUiData.GetEmployeeSummariesAsync(null, cancellationToken);
        var outstandingRows = employees.Where(e => e.OutstandingBalance > 0m).ToList();
        var ledgerRows = await _advancesUiData.GetAllLedgerEntriesAsync(staffId, cancellationToken: cancellationToken);
        var recoveryRows = await _advancesUiData.GetMonthlyRecoveryReportAsync(selectedMonth, selectedYear, cancellationToken);

        var viewModel = new AdvanceReportsViewModel
        {
            ReportType = reportType,
            StaffId = staffId,
            Month = selectedMonth,
            Year = selectedYear,
            Employees = employees,
            OutstandingRows = outstandingRows,
            LedgerRows = ledgerRows,
            RecoveryRows = recoveryRows,
            TotalOutstanding = outstandingRows.Sum(r => r.OutstandingBalance),
            TotalRecovered = recoveryRows.Sum(r => r.RecoveredAmount)
        };

        return View(viewModel);
    }

    private IActionResult HandleMutationFailure(long staffId, string errorMessage)
    {
        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
        {
            return Json(new { isSuccess = false, error = errorMessage });
        }

        TempData["ErrorMessage"] = errorMessage;
        return RedirectToAction(nameof(Index), new { staffId });
    }

    private bool HasPermission(string permission)
    {
        var isSuper = _currentUser.Roles.Contains("Owner", StringComparer.OrdinalIgnoreCase) ||
                      _currentUser.Roles.Contains("Admin", StringComparer.OrdinalIgnoreCase) ||
                      _currentUser.Roles.Count == 0;
        return isSuper || _currentUser.Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }
}

using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Application.Common.Models.Staff;
using RestaurantMS.Web.Models;

namespace RestaurantMS.Web.Controllers;

[Authorize]
public class StaffController : Controller
{
    private readonly IStaffUiData _staffUiData;
    private readonly IEmployeeAdvancesUiData _advancesUiData;
    private readonly ISettingService _settingService;
    private readonly IDateTime _dateTime;

    public StaffController(
        IStaffUiData staffUiData,
        IEmployeeAdvancesUiData advancesUiData,
        ISettingService settingService,
        IDateTime dateTime)
    {
        _staffUiData = staffUiData;
        _advancesUiData = advancesUiData;
        _settingService = settingService;
        _dateTime = dateTime;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? tab = null,
        DateTime? date = null,
        int? month = null,
        int? year = null,
        long? calendarStaffId = null,
        bool? autoRecover = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var activeTab = NormalizeTab(tab);
        var viewModel = await BuildHubViewModelAsync(activeTab, date, month, year, calendarStaffId, autoRecover ?? true, search, cancellationToken);
        return View("Index", viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Attendance(
        [FromQuery] DateTime? date = null,
        [FromQuery] int? month = null,
        [FromQuery] int? year = null,
        [FromQuery] long? calendarStaffId = null,
        CancellationToken cancellationToken = default)
    {
        var viewModel = await BuildHubViewModelAsync("attendance", date, month, year, calendarStaffId, true, null, cancellationToken);
        return View("Index", viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> SalaryReport(
        [FromQuery] int? month = null,
        [FromQuery] int? year = null,
        [FromQuery] bool? autoRecover = null,
        CancellationToken cancellationToken = default)
    {
        var viewModel = await BuildHubViewModelAsync("salary", null, month, year, null, autoRecover ?? true, null, cancellationToken);
        return View("Index", viewModel);
    }

    [HttpGet]
    public IActionResult Advances()
    {
        return RedirectToActionPermanent("Index", "EmployeeAdvances");
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new StaffFormViewModel { JoiningDate = _dateTime.UtcNow.Date });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StaffFormViewModel model, string? submitAction = null, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var saved = await _staffUiData.SaveStaffProfileAsync(MapToProfile(model), cancellationToken);
        TempData["SuccessMessage"] = $"Staff member '{saved.Name}' saved successfully.";
        TempData["HighlightedRow"] = saved.EmployeeCode;
        if (string.Equals(submitAction, "saveAndNew", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(Create));
        }

        return RedirectToAction(nameof(Index), new { tab = "list" });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(long id, CancellationToken cancellationToken)
    {
        var profile = await _staffUiData.GetStaffProfileByIdAsync(id, cancellationToken);
        if (profile is null)
        {
            return NotFound();
        }

        var model = new StaffFormViewModel
        {
            Id = profile.Id,
            EmployeeCode = profile.EmployeeCode,
            FullName = profile.Name,
            Role = profile.Designation,
            MobileNumber = profile.Mobile,
            Shift = profile.Shift,
            SalaryType = profile.SalaryType,
            MonthlySalary = profile.BasicSalary,
            JoiningDate = profile.JoiningDate,
            IdProof = profile.IdProof,
            BankOrUpi = profile.BankOrUpi,
            PhotoUrl = profile.PhotoUrl,
            IsActive = profile.IsActive
        };

        return View("Create", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(StaffFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("Create", model);
        }

        await _staffUiData.SaveStaffProfileAsync(MapToProfile(model), cancellationToken);
        TempData["SuccessMessage"] = $"Staff profile '{model.FullName}' updated successfully.";
        return RedirectToAction(nameof(Details), new { id = model.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Details(long id, string? tab = null, int? month = null, int? year = null, CancellationToken cancellationToken = default)
    {
        var profile = await _staffUiData.GetStaffProfileByIdAsync(id, cancellationToken);
        if (profile is null)
        {
            var all = await _staffUiData.GetStaffProfilesAsync(cancellationToken: cancellationToken);
            profile = all.FirstOrDefault();
            if (profile is null) return NotFound();
        }

        var now = _dateTime.UtcNow;
        var selectedMonth = month ?? now.Month;
        var selectedYear = year ?? now.Year;
        var activeTab = (tab ?? "details").Trim().ToLowerInvariant();
        if (activeTab is not ("details" or "attendance" or "advances" or "salary"))
        {
            activeTab = "details";
        }

        var calendar = await _staffUiData.GetMonthlyCalendarAsync(profile.Id, selectedMonth, selectedYear, cancellationToken);
        var advSummary = await _advancesUiData.GetEmployeeSummaryAsync(profile.Id, cancellationToken);
        var advLedger = await _advancesUiData.GetEmployeeLedgerAsync(profile.Id, cancellationToken);
        var salaryHistory = await _staffUiData.GetStaffSalaryHistoryAsync(profile.Id, cancellationToken);

        var viewModel = new StaffProfilePageViewModel
        {
            ActiveTab = activeTab,
            Profile = profile,
            SelectedMonth = selectedMonth,
            SelectedYear = selectedYear,
            MonthlyCalendar = calendar,
            AdvanceSummary = advSummary,
            RecentAdvances = advLedger.Take(5).ToList(),
            SalaryHistory = salaryHistory
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveAttendance(
        long staffId,
        DateTime date,
        string status,
        string inTime,
        string outTime,
        decimal overtimeHours,
        string? remarks,
        CancellationToken cancellationToken)
    {
        var result = await _staffUiData.SaveAttendanceEntryAsync(staffId, date, status, inTime, outTime, overtimeHours, remarks ?? string.Empty, cancellationToken);
        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
        {
            return Json(new { isSuccess = result.IsSuccess, value = result.IsSuccess ? result.Value : false, error = result.IsFailure ? result.Error.Description : null });
        }

        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Error.Description;
        }
        else
        {
            TempData["SuccessMessage"] = "Attendance updated.";
        }

        return RedirectToAction(nameof(Index), new { tab = "attendance", date = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllPresent(DateTime date, CancellationToken cancellationToken)
    {
        var result = await _staffUiData.MarkAllPresentAsync(date, cancellationToken);
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Error.Description;
        }
        else
        {
            TempData["SuccessMessage"] = $"Marked {result.Value} active staff members Present for {date:dd MMM yyyy}.";
        }

        return RedirectToAction(nameof(Index), new { tab = "attendance", date = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleMonthLock(int month, int year, bool locked, string? returnTab, CancellationToken cancellationToken)
    {
        var result = await _staffUiData.SetMonthLockAsync(month, year, locked, cancellationToken);
        var monthLabel = new DateTime(year, month, 1).ToString("MMMM yyyy", CultureInfo.InvariantCulture);
        TempData["SuccessMessage"] = result.Value
            ? $"Payroll month {monthLabel} is now LOCKED. Attendance cannot be edited."
            : $"Payroll month {monthLabel} unlocked.";

        return RedirectToAction(nameof(Index), new { tab = NormalizeTab(returnTab), month, year });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkSalaryPaid(long staffId, int month, int year, bool autoRecover, CancellationToken cancellationToken)
    {
        var result = await _staffUiData.MarkSalaryPaidAsync(staffId, month, year, autoRecover, cancellationToken);
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Error.Description;
        }
        else
        {
            var voucherNote = !string.IsNullOrEmpty(result.Value.RecoveryVoucherNumber)
                ? $" Advance recovery posted ({result.Value.RecoveryVoucherNumber})."
                : string.Empty;
            TempData["SuccessMessage"] = $"Salary marked Paid for {result.Value.EmployeeName} (Net ₹{result.Value.NetPayable:N2}).{voucherNote}";
        }

        return RedirectToAction(nameof(Index), new { tab = "salary", month, year, autoRecover });
    }

    [HttpGet]
    public async Task<IActionResult> Payslip(long staffId, int? month = null, int? year = null, bool? autoRecover = null, CancellationToken cancellationToken = default)
    {
        var now = _dateTime.UtcNow;
        var selectedMonth = month ?? now.Month;
        var selectedYear = year ?? now.Year;

        var profile = await _staffUiData.GetStaffProfileByIdAsync(staffId, cancellationToken);
        var salary = await _staffUiData.GetPayslipAsync(staffId, selectedMonth, selectedYear, autoRecover ?? true, cancellationToken);
        if (profile is null || salary is null)
        {
            return NotFound();
        }

        var hotelName = await _settingService.GetSettingAsync("HotelNameEn", "Grand Restaurant", cancellationToken);
        var hotelAddress = await _settingService.GetSettingAsync("Address", "123 Main Street, Pune, MH, India", cancellationToken);

        var viewModel = new PayslipViewModel
        {
            HotelName = hotelName,
            HotelAddress = hotelAddress,
            Profile = profile,
            Salary = salary,
            MonthYearLabel = new DateTime(selectedYear, selectedMonth, 1).ToString("MMMM yyyy", CultureInfo.InvariantCulture)
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult EmployeeReport([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var fromDate = from ?? _dateTime.UtcNow.AddDays(-30);
        var toDate = to ?? _dateTime.UtcNow;

        var employees = new List<EmployeePerformanceRowModel>
        {
            new() { StaffId = 1, EmployeeName = "Suresh Patil", Designation = "Head Waiter", ShiftsAttended = 29, AttendancePercentage = 96.6m, TablesServed = 312, OrdersPunched = 428, TotalSalesValue = 284500, Rating = 4.9m },
            new() { StaffId = 2, EmployeeName = "Anil Deshmukh", Designation = "Waiter", ShiftsAttended = 27, AttendancePercentage = 90.0m, TablesServed = 245, OrdersPunched = 310, TotalSalesValue = 198000, Rating = 4.7m },
            new() { StaffId = 3, EmployeeName = "Meena Jadhav", Designation = "Captain", ShiftsAttended = 30, AttendancePercentage = 100.0m, TablesServed = 380, OrdersPunched = 520, TotalSalesValue = 365000, Rating = 5.0m },
            new() { StaffId = 4, EmployeeName = "Chef Rakesh", Designation = "Head Chef", ShiftsAttended = 28, AttendancePercentage = 93.3m, TablesServed = 0, OrdersPunched = 650, TotalSalesValue = 490000, Rating = 4.9m },
            new() { StaffId = 5, EmployeeName = "Prakash Shinde", Designation = "Cashier", ShiftsAttended = 30, AttendancePercentage = 100.0m, TablesServed = 0, OrdersPunched = 720, TotalSalesValue = 560000, Rating = 4.8m },
            new() { StaffId = 6, EmployeeName = "Kavita Pawar", Designation = "Kitchen Assistant", ShiftsAttended = 26, AttendancePercentage = 86.6m, TablesServed = 0, OrdersPunched = 210, TotalSalesValue = 110000, Rating = 4.6m }
        };

        return View(new EmployeeReportViewModel { FromDate = fromDate, ToDate = toDate, Employees = employees });
    }

    private async Task<StaffHubViewModel> BuildHubViewModelAsync(
        string activeTab,
        DateTime? date,
        int? month,
        int? year,
        long? calendarStaffId,
        bool autoRecover,
        string? search,
        CancellationToken cancellationToken)
    {
        var today = _dateTime.UtcNow.Date;
        var selectedDate = date?.Date ?? today;
        var selectedMonth = month ?? selectedDate.Month;
        var selectedYear = year ?? selectedDate.Year;

        var profiles = await _staffUiData.GetStaffProfilesAsync(search, cancellationToken);
        var allProfiles = string.IsNullOrWhiteSpace(search)
            ? profiles
            : await _staffUiData.GetStaffProfilesAsync(null, cancellationToken);

        var dailyAttendance = await _staffUiData.GetDailyAttendanceAsync(selectedDate, cancellationToken);
        var calStaffId = calendarStaffId ?? allProfiles.FirstOrDefault(p => p.IsActive)?.Id ?? 1;
        var calendar = await _staffUiData.GetMonthlyCalendarAsync(calStaffId, selectedMonth, selectedYear, cancellationToken);
        var isLocked = await _staffUiData.IsMonthLockedAsync(selectedMonth, selectedYear, cancellationToken);
        var salaryRows = await _staffUiData.GetSalaryStatementAsync(selectedMonth, selectedYear, autoRecover, cancellationToken);

        return new StaffHubViewModel
        {
            ActiveTab = activeTab,
            Search = search,
            TotalStaff = allProfiles.Count,
            ActiveStaff = allProfiles.Count(p => p.IsActive),
            PresentToday = dailyAttendance.Count(a => a.Status is "Present" or "HalfDay"),
            MonthlyPayrollEstimate = salaryRows.Sum(s => s.NetPayable),
            StaffProfiles = profiles,
            SelectedDate = selectedDate,
            SelectedMonth = selectedMonth,
            SelectedYear = selectedYear,
            SelectedCalendarStaffId = calStaffId,
            IsMonthLocked = isLocked,
            DailyAttendance = dailyAttendance,
            MonthlyCalendar = calendar,
            AutoRecoverAdvance = autoRecover,
            SalaryRows = salaryRows,
            TotalNetPayroll = salaryRows.Sum(s => s.NetPayable),
            TotalAdvanceRecovery = salaryRows.Sum(s => s.AdvanceRecovery)
        };
    }

    private static string NormalizeTab(string? tab)
    {
        var normalized = (tab ?? "list").Trim().ToLowerInvariant();
        return normalized is "list" or "attendance" or "salary" ? normalized : "list";
    }

    private static StaffProfileDto MapToProfile(StaffFormViewModel model) => new(
        model.Id,
        model.EmployeeCode,
        model.FullName,
        model.MobileNumber,
        model.Role,
        model.Shift,
        model.JoiningDate,
        model.SalaryType,
        model.MonthlySalary,
        model.IdProof,
        model.BankOrUpi,
        model.PhotoUrl,
        model.IsActive,
        string.Equals(model.SalaryType, "Daily", StringComparison.OrdinalIgnoreCase) ? model.MonthlySalary * 30m : model.MonthlySalary);
}

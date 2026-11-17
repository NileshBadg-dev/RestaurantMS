using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Domain.Constants;
using RestaurantMS.Web.Models;

namespace RestaurantMS.Web.Controllers;

[Authorize]
public class StaffController : Controller
{
    private readonly IStaffUiData _staffUiData;

    public StaffController(IStaffUiData staffUiData)
    {
        _staffUiData = staffUiData;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var staffMembers = await _staffUiData.GetAllStaffAsync(cancellationToken);

        var viewModel = new StaffListViewModel
        {
            StaffMembers = staffMembers,
            TotalStaff = staffMembers.Count,
            PresentToday = staffMembers.Count(s => s.IsOnDuty),
            OnLeaveToday = staffMembers.Count(s => !s.IsOnDuty)
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var viewModel = new StaffFormViewModel();
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StaffFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _staffUiData.SaveStaffMemberAsync(model.FullName, model.Role, model.MobileNumber, model.Shift, cancellationToken);
        TempData["SuccessMessage"] = $"Staff member '{model.FullName}' added successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(long id, CancellationToken cancellationToken)
    {
        var all = await _staffUiData.GetAllStaffAsync(cancellationToken);
        var staff = all.FirstOrDefault(s => s.Id == id) ?? all.FirstOrDefault();

        var viewModel = new StaffDetailsViewModel
        {
            Id = staff?.Id ?? id,
            EmployeeCode = staff?.EmployeeCode ?? $"EMP-{id:D3}",
            FullName = staff?.Name ?? "Suresh Patil",
            Role = staff?.Role ?? "Head Waiter",
            MobileNumber = staff?.Mobile ?? "9822114455",
            Shift = staff?.Shift ?? "Dinner (04:00 PM - 11:30 PM)",
            Email = $"{(staff?.Name ?? "staff").ToLowerInvariant().Replace(" ", ".")}@restaurant.local",
            MonthlySalary = staff?.Role switch
            {
                "Head Chef" => 35000m,
                "Captain" => 24000m,
                "Cashier" => 20000m,
                "Head Waiter" => 22000m,
                "Kitchen Assistant" => 14000m,
                _ => 16000m
            },
            TotalAdvanceBalance = 2500m,
            JoiningDate = new DateTime(2024, 2, 1),
            BankAccountOrUpi = $"{staff?.Mobile ?? "9822114455"}@okhdfc / HDFC0001234",
            AadhaarNumber = "XXXX-XXXX-4812",
            EmergencyContact = "Pravin Patil (Brother) - 9822998877",
            DaysPresentThisMonth = 25,
            DaysAbsentThisMonth = 2,
            TotalSalesHandled = 184500m
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Attendance([FromQuery] DateTime? date, CancellationToken cancellationToken)
    {
        var selectedDate = date ?? DateTime.UtcNow;
        var staffMembers = await _staffUiData.GetAllStaffAsync(cancellationToken);

        var rows = staffMembers.Select(s => new StaffAttendanceRowModel
        {
            StaffId = s.Id,
            EmployeeCode = s.EmployeeCode,
            FullName = s.Name,
            Role = s.Role,
            Shift = s.Shift,
            Status = s.IsOnDuty ? "Present" : "Leave",
            InTime = s.IsOnDuty ? "10:15 AM" : "-",
            OutTime = s.IsOnDuty ? "10:30 PM" : "-",
            HoursWorked = s.IsOnDuty ? 8.5m : 0.0m,
            Remarks = s.IsOnDuty ? "On Duty" : "Approved Leave"
        }).ToList();

        var viewModel = new StaffAttendanceViewModel
        {
            AttendanceDate = selectedDate,
            AttendanceList = rows
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Advances()
    {
        var advances = new List<StaffAdvanceRowModel>
        {
            new() { Id = 1, VoucherNumber = "ADV-2026-001", DateFormatted = "12 Nov 2026", EmployeeName = "Suresh Patil", Role = "Head Waiter", AdvanceAmount = 5000, RecoveredAmount = 2500, PaymentMode = "Cash", Reason = "Family Medical Emergency", Status = "PartiallyDeducted" },
            new() { Id = 2, VoucherNumber = "ADV-2026-002", DateFormatted = "14 Nov 2026", EmployeeName = "Anil Deshmukh", Role = "Waiter", AdvanceAmount = 3000, RecoveredAmount = 0, PaymentMode = "UPI", Reason = "Diwali Festival Expenses", Status = "Pending" },
            new() { Id = 3, VoucherNumber = "ADV-2026-003", DateFormatted = "05 Nov 2026", EmployeeName = "Meena Jadhav", Role = "Captain", AdvanceAmount = 4000, RecoveredAmount = 4000, PaymentMode = "Cash", Reason = "Travel & Commute Expense", Status = "Settled" },
            new() { Id = 4, VoucherNumber = "ADV-2026-004", DateFormatted = "16 Nov 2026", EmployeeName = "Chef Rakesh", Role = "Head Chef", AdvanceAmount = 10000, RecoveredAmount = 2000, PaymentMode = "Bank Transfer", Reason = "Child School Fees", Status = "PartiallyDeducted" },
            new() { Id = 5, VoucherNumber = "ADV-2026-005", DateFormatted = "10 Nov 2026", EmployeeName = "Prakash Shinde", Role = "Cashier", AdvanceAmount = 2000, RecoveredAmount = 2000, PaymentMode = "Cash", Reason = "Urgent Personal Advance", Status = "Settled" }
        };

        var viewModel = new StaffAdvanceViewModel
        {
            Advances = advances
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RecordAdvance(string employeeName, decimal amount, string paymentMode, string reason)
    {
        TempData["SuccessMessage"] = $"Advance of ₹{amount:N0} recorded for '{employeeName}' successfully!";
        return RedirectToAction(nameof(Advances));
    }

    [HttpGet]
    public IActionResult SalaryReport([FromQuery] int? month, [FromQuery] int? year)
    {
        var selectedMonth = month ?? DateTime.UtcNow.Month;
        var selectedYear = year ?? DateTime.UtcNow.Year;

        var salaries = new List<SalaryRowModel>
        {
            new() { StaffId = 1, EmployeeCode = "EMP-001", EmployeeName = "Suresh Patil", Designation = "Head Waiter", MonthlyBaseSalary = 22000, TotalDaysInMonth = 30, AttendedDays = 29, GrossEarnedSalary = 21266, AdvanceDeduction = 2500, OvertimeBonus = 1200, PaymentStatus = "Pending" },
            new() { StaffId = 2, EmployeeCode = "EMP-002", EmployeeName = "Anil Deshmukh", Designation = "Waiter", MonthlyBaseSalary = 16000, TotalDaysInMonth = 30, AttendedDays = 27, GrossEarnedSalary = 14400, AdvanceDeduction = 1500, OvertimeBonus = 600, PaymentStatus = "Paid", PaymentDate = "01 Nov 2026" },
            new() { StaffId = 3, EmployeeCode = "EMP-003", EmployeeName = "Meena Jadhav", Designation = "Captain", MonthlyBaseSalary = 24000, TotalDaysInMonth = 30, AttendedDays = 30, GrossEarnedSalary = 24000, AdvanceDeduction = 0, OvertimeBonus = 2000, PaymentStatus = "Paid", PaymentDate = "01 Nov 2026" },
            new() { StaffId = 4, EmployeeCode = "EMP-004", EmployeeName = "Chef Rakesh", Designation = "Head Chef", MonthlyBaseSalary = 35000, TotalDaysInMonth = 30, AttendedDays = 28, GrossEarnedSalary = 32666, AdvanceDeduction = 4000, OvertimeBonus = 3500, PaymentStatus = "Pending" },
            new() { StaffId = 5, EmployeeCode = "EMP-005", EmployeeName = "Prakash Shinde", Designation = "Cashier", MonthlyBaseSalary = 20000, TotalDaysInMonth = 30, AttendedDays = 30, GrossEarnedSalary = 20000, AdvanceDeduction = 1000, OvertimeBonus = 800, PaymentStatus = "Paid", PaymentDate = "01 Nov 2026" },
            new() { StaffId = 6, EmployeeCode = "EMP-006", EmployeeName = "Kavita Pawar", Designation = "Kitchen Assistant", MonthlyBaseSalary = 14000, TotalDaysInMonth = 30, AttendedDays = 26, GrossEarnedSalary = 12133, AdvanceDeduction = 500, OvertimeBonus = 400, PaymentStatus = "Pending" }
        };

        var viewModel = new SalaryReportViewModel
        {
            Month = selectedMonth,
            Year = selectedYear,
            Salaries = salaries
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult EmployeeReport([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
        var toDate = to ?? DateTime.UtcNow;

        var employees = new List<EmployeePerformanceRowModel>
        {
            new() { StaffId = 1, EmployeeName = "Suresh Patil", Designation = "Head Waiter", ShiftsAttended = 29, AttendancePercentage = 96.6m, TablesServed = 312, OrdersPunched = 428, TotalSalesValue = 284500, Rating = 4.9m },
            new() { StaffId = 2, EmployeeName = "Anil Deshmukh", Designation = "Waiter", ShiftsAttended = 27, AttendancePercentage = 90.0m, TablesServed = 245, OrdersPunched = 310, TotalSalesValue = 198000, Rating = 4.7m },
            new() { StaffId = 3, EmployeeName = "Meena Jadhav", Designation = "Captain", ShiftsAttended = 30, AttendancePercentage = 100.0m, TablesServed = 380, OrdersPunched = 520, TotalSalesValue = 365000, Rating = 5.0m },
            new() { StaffId = 4, EmployeeName = "Chef Rakesh", Designation = "Head Chef", ShiftsAttended = 28, AttendancePercentage = 93.3m, TablesServed = 0, OrdersPunched = 650, TotalSalesValue = 490000, Rating = 4.9m },
            new() { StaffId = 5, EmployeeName = "Prakash Shinde", Designation = "Cashier", ShiftsAttended = 30, AttendancePercentage = 100.0m, TablesServed = 0, OrdersPunched = 720, TotalSalesValue = 560000, Rating = 4.8m },
            new() { StaffId = 6, EmployeeName = "Kavita Pawar", Designation = "Kitchen Assistant", ShiftsAttended = 26, AttendancePercentage = 86.6m, TablesServed = 0, OrdersPunched = 210, TotalSalesValue = 110000, Rating = 4.6m }
        };

        var viewModel = new EmployeeReportViewModel
        {
            FromDate = fromDate,
            ToDate = toDate,
            Employees = employees
        };

        return View(viewModel);
    }
}

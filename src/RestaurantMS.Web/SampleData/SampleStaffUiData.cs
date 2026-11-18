using System.Globalization;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Application.Common.Models;
using RestaurantMS.Application.Common.Models.Staff;
using RestaurantMS.Application.Common.Models.UiData;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Web.SampleData;

public class SampleStaffUiData : IStaffUiData
{
    private readonly SampleStaffStore _staffStore;
    private readonly SampleAttendanceSalaryStore _store;
    private readonly IEmployeeAdvancesUiData _advancesUiData;
    private readonly ISalaryCalculationService _salaryCalculator;
    private readonly IDateTime _dateTime;

    public SampleStaffUiData(
        SampleStaffStore staffStore,
        SampleAttendanceSalaryStore store,
        IEmployeeAdvancesUiData advancesUiData,
        ISalaryCalculationService salaryCalculator,
        IDateTime dateTime)
    {
        _staffStore = staffStore;
        _store = store;
        _advancesUiData = advancesUiData;
        _salaryCalculator = salaryCalculator;
        _dateTime = dateTime;
    }

    public Task<PagedResult<StaffMemberDto>> GetStaffListAsync(DataTableRequest request, CancellationToken cancellationToken = default)
    {
        var all = _staffStore.GetAll()
            .Select(s => new StaffMemberDto(s.Id, s.EmployeeCode, s.Name, s.Designation, s.Mobile, s.Shift, s.IsActive))
            .ToList();

        if (!string.IsNullOrWhiteSpace(request.SearchValue))
        {
            var search = request.SearchValue.Trim().ToLowerInvariant();
            all = all.Where(s => s.Name.ToLowerInvariant().Contains(search) || s.Role.ToLowerInvariant().Contains(search)).ToList();
        }

        var paged = all.Skip(request.Start).Take(request.PageSize).ToList();
        return Task.FromResult(new PagedResult<StaffMemberDto>(paged, all.Count, request.PageNumber, request.PageSize));
    }

    public Task<IReadOnlyList<StaffMemberDto>> GetAllStaffAsync(CancellationToken cancellationToken = default)
    {
        var list = _staffStore.GetAll()
            .Select(s => new StaffMemberDto(s.Id, s.EmployeeCode, s.Name, s.Designation, s.Mobile, s.Shift, s.IsActive))
            .ToList();
        return Task.FromResult<IReadOnlyList<StaffMemberDto>>(list);
    }

    public Task<IReadOnlyList<StaffProfileDto>> GetStaffProfilesAsync(string? search = null, CancellationToken cancellationToken = default)
    {
        var all = _staffStore.GetAll();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            all = all.Where(s =>
                s.Name.ToLowerInvariant().Contains(term) ||
                s.EmployeeCode.ToLowerInvariant().Contains(term) ||
                s.Designation.ToLowerInvariant().Contains(term) ||
                s.Mobile.Contains(term)).ToList();
        }

        return Task.FromResult(all);
    }

    public Task<StaffProfileDto?> GetStaffProfileByIdAsync(long staffId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_staffStore.GetById(staffId));
    }

    public Task SaveStaffMemberAsync(string name, string role, string mobile, string shift, CancellationToken cancellationToken = default)
    {
        _staffStore.Upsert(new StaffProfileDto(
            0,
            string.Empty,
            name,
            mobile,
            role,
            shift,
            _dateTime.UtcNow.Date,
            "Monthly",
            16000m,
            "Pending Verification",
            string.Empty,
            null,
            true,
            16000m));
        return Task.CompletedTask;
    }

    public Task<StaffProfileDto> SaveStaffProfileAsync(StaffProfileDto profile, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_staffStore.Upsert(profile));
    }

    public Task<IReadOnlyList<AttendanceEntryDto>> GetDailyAttendanceAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var targetDate = date.Date;
        var staffList = _staffStore.GetAll().Where(s => s.IsActive).ToList();

        lock (_store.SyncRoot)
        {
            var results = new List<AttendanceEntryDto>();
            foreach (var staff in staffList)
            {
                if (_store.AttendanceByKey.TryGetValue((staff.Id, targetDate), out var existing))
                {
                    results.Add(existing);
                }
                else
                {
                    results.Add(BuildDefaultAttendance(staff, targetDate));
                }
            }
            return Task.FromResult<IReadOnlyList<AttendanceEntryDto>>(results);
        }
    }

    public Task<Result<bool>> SaveAttendanceEntryAsync(
        long staffId,
        DateTime date,
        string status,
        string inTime,
        string outTime,
        decimal overtimeHours,
        string remarks,
        CancellationToken cancellationToken = default)
    {
        var targetDate = date.Date;
        lock (_store.SyncRoot)
        {
            if (_store.LockedMonths.Contains((targetDate.Year, targetDate.Month)))
            {
                return Task.FromResult(Result.Failure<bool>(new Error(
                    "Attendance.MonthLocked",
                    $"Attendance for {targetDate:MMMM yyyy} is locked after payroll processing.")));
            }

            var staff = _staffStore.GetById(staffId);
            if (staff is null)
            {
                return Task.FromResult(Result.Failure<bool>(new Error("Staff.NotFound", "Staff member not found.")));
            }

            _store.AttendanceByKey[(staffId, targetDate)] = new AttendanceEntryDto(
                staff.Id,
                staff.EmployeeCode,
                staff.Name,
                staff.Designation,
                staff.Shift,
                targetDate,
                status,
                string.IsNullOrWhiteSpace(inTime) ? "-" : inTime,
                string.IsNullOrWhiteSpace(outTime) ? "-" : outTime,
                Math.Max(0m, overtimeHours),
                remarks ?? string.Empty);

            return Task.FromResult(Result.Success(true));
        }
    }

    public Task<Result<int>> MarkAllPresentAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var targetDate = date.Date;
        lock (_store.SyncRoot)
        {
            if (_store.LockedMonths.Contains((targetDate.Year, targetDate.Month)))
            {
                return Task.FromResult(Result.Failure<int>(new Error(
                    "Attendance.MonthLocked",
                    $"Attendance for {targetDate:MMMM yyyy} is locked after payroll processing.")));
            }

            var activeStaff = _staffStore.GetAll().Where(s => s.IsActive).ToList();
            foreach (var staff in activeStaff)
            {
                _store.AttendanceByKey[(staff.Id, targetDate)] = new AttendanceEntryDto(
                    staff.Id,
                    staff.EmployeeCode,
                    staff.Name,
                    staff.Designation,
                    staff.Shift,
                    targetDate,
                    "Present",
                    "10:00",
                    "22:00",
                    0m,
                    "Bulk marked present");
            }

            return Task.FromResult(Result.Success(activeStaff.Count));
        }
    }

    public Task<IReadOnlyList<MonthlyCalendarDayDto>> GetMonthlyCalendarAsync(long staffId, int month, int year, CancellationToken cancellationToken = default)
    {
        var staff = _staffStore.GetById(staffId) ?? _staffStore.GetAll().First();
        var daysInMonth = DateTime.DaysInMonth(year, month);
        var days = new List<MonthlyCalendarDayDto>(daysInMonth);

        lock (_store.SyncRoot)
        {
            for (var day = 1; day <= daysInMonth; day++)
            {
                var dt = new DateTime(year, month, day);
                var entry = _store.AttendanceByKey.TryGetValue((staff.Id, dt), out var saved)
                    ? saved
                    : BuildDefaultAttendance(staff, dt);

                days.Add(new MonthlyCalendarDayDto(
                    dt,
                    day,
                    dt.ToString("ddd", CultureInfo.InvariantCulture),
                    entry.Status,
                    ToShortCode(entry.Status),
                    entry.InTime,
                    entry.OutTime,
                    entry.OvertimeHours));
            }
        }

        return Task.FromResult<IReadOnlyList<MonthlyCalendarDayDto>>(days);
    }

    public Task<bool> IsMonthLockedAsync(int month, int year, CancellationToken cancellationToken = default)
    {
        lock (_store.SyncRoot)
        {
            return Task.FromResult(_store.LockedMonths.Contains((year, month)));
        }
    }

    public Task<Result<bool>> SetMonthLockAsync(int month, int year, bool locked, CancellationToken cancellationToken = default)
    {
        lock (_store.SyncRoot)
        {
            if (locked)
            {
                _store.LockedMonths.Add((year, month));
            }
            else
            {
                _store.LockedMonths.Remove((year, month));
            }
            return Task.FromResult(Result.Success(locked));
        }
    }

    public async Task<IReadOnlyList<SalaryStatementRowDto>> GetSalaryStatementAsync(int month, int year, bool autoRecoverAdvance, CancellationToken cancellationToken = default)
    {
        var staffList = _staffStore.GetAll().Where(s => s.IsActive).ToList();
        var rows = new List<SalaryStatementRowDto>();

        foreach (var staff in staffList)
        {
            var row = await ComputeSalaryRowAsync(staff, month, year, autoRecoverAdvance, cancellationToken);
            rows.Add(row);
        }

        return rows;
    }

    public async Task<SalaryStatementRowDto?> GetPayslipAsync(long staffId, int month, int year, bool autoRecoverAdvance, CancellationToken cancellationToken = default)
    {
        var staff = _staffStore.GetById(staffId);
        if (staff is null) return null;
        return await ComputeSalaryRowAsync(staff, month, year, autoRecoverAdvance, cancellationToken);
    }

    public async Task<IReadOnlyList<SalaryStatementRowDto>> GetStaffSalaryHistoryAsync(long staffId, CancellationToken cancellationToken = default)
    {
        var staff = _staffStore.GetById(staffId);
        if (staff is null) return [];

        var now = _dateTime.UtcNow;
        var list = new List<SalaryStatementRowDto>();
        for (var i = 0; i < 3; i++)
        {
            var target = now.AddMonths(-i);
            var row = await ComputeSalaryRowAsync(staff, target.Month, target.Year, autoRecoverAdvance: true, cancellationToken);
            if (i > 0 && row.Status == "Draft")
            {
                row = row with { Status = "Paid", PaidDate = new DateTime(target.Year, target.Month, DateTime.DaysInMonth(target.Year, target.Month)) };
            }
            list.Add(row);
        }
        return list;
    }

    public async Task<Result<SalaryStatementRowDto>> MarkSalaryPaidAsync(long staffId, int month, int year, bool autoRecoverAdvance, CancellationToken cancellationToken = default)
    {
        var staff = _staffStore.GetById(staffId);
        if (staff is null)
        {
            return Result.Failure<SalaryStatementRowDto>(new Error("Staff.NotFound", "Staff member not found."));
        }

        var row = await ComputeSalaryRowAsync(staff, month, year, autoRecoverAdvance, cancellationToken);
        if (row.Status == "Paid")
        {
            return Result.Success(row);
        }

        string? recoveryVoucher = null;
        decimal recoveredAmount = 0m;

        if (autoRecoverAdvance && row.AdvanceRecovery > 0m)
        {
            var recoveryResult = await _advancesUiData.RecordRecoveryAsync(
                new RecordAdvanceRecoveryRequest
                {
                    StaffId = staffId,
                    Date = _dateTime.UtcNow.Date,
                    Amount = row.AdvanceRecovery,
                    PaymentMode = "SalaryDeduction",
                    Note = $"Auto-recovery from {new DateTime(year, month, 1):MMM yyyy} salary statement"
                },
                cancellationToken);

            if (recoveryResult.IsFailure)
            {
                return Result.Failure<SalaryStatementRowDto>(recoveryResult.Error);
            }

            recoveryVoucher = recoveryResult.Value.VoucherNumber;
            recoveredAmount = recoveryResult.Value.Amount;
        }

        var paidDate = _dateTime.UtcNow.Date;
        lock (_store.SyncRoot)
        {
            _store.PayrollStatus[(staffId, year, month)] = ("Paid", paidDate, recoveryVoucher, recoveredAmount);
        }

        return Result.Success(row with
        {
            Status = "Paid",
            PaidDate = paidDate,
            RecoveryVoucherNumber = recoveryVoucher
        });
    }

    private async Task<SalaryStatementRowDto> ComputeSalaryRowAsync(StaffProfileDto staff, int month, int year, bool autoRecoverAdvance, CancellationToken cancellationToken)
    {
        var calendar = await GetMonthlyCalendarAsync(staff.Id, month, year, cancellationToken);
        var advSummary = await _advancesUiData.GetEmployeeSummaryAsync(staff.Id, cancellationToken);

        var totalDays = calendar.Count;
        var presentDays = calendar.Count(d => d.Status == "Present");
        var absentDays = calendar.Count(d => d.Status == "Absent");
        var halfDays = calendar.Count(d => d.Status == "HalfDay");
        var leaveDays = calendar.Count(d => d.Status == "Leave");
        var weekOffDays = calendar.Count(d => d.Status == "WeekOff");
        var overtimeHours = calendar.Sum(d => d.OvertimeHours);

        (string Status, DateTime? PaidDate, string? RecoveryVoucher, decimal PaidRecoveryAmount) payroll;
        bool hasPaidRecord;
        lock (_store.SyncRoot)
        {
            hasPaidRecord = _store.PayrollStatus.TryGetValue((staff.Id, year, month), out payroll);
        }

        var outstanding = advSummary?.OutstandingBalance ?? 0m;
        var plannedInstalment = advSummary?.PlannedMonthlyInstalment ?? 0m;
        var useAutoRecover = autoRecoverAdvance && (advSummary?.AutoRecoverFromSalary ?? true);

        var calc = _salaryCalculator.Calculate(new SalaryCalculationInput(
            staff.SalaryType,
            staff.BasicSalary,
            totalDays,
            presentDays,
            absentDays,
            halfDays,
            leaveDays,
            weekOffDays,
            overtimeHours,
            1.5m,
            hasPaidRecord ? payroll.PaidRecoveryAmount : outstanding,
            hasPaidRecord ? payroll.PaidRecoveryAmount : plannedInstalment,
            hasPaidRecord ? payroll.PaidRecoveryAmount > 0m : useAutoRecover));

        return new SalaryStatementRowDto(
            staff.Id,
            staff.EmployeeCode,
            staff.Name,
            staff.Designation,
            staff.SalaryType,
            month,
            year,
            totalDays,
            presentDays,
            absentDays,
            halfDays,
            overtimeHours,
            calc.BaseSalaryForMonth,
            calc.OvertimePay,
            calc.AbsentDeduction,
            outstanding,
            calc.AdvanceRecovery,
            calc.NetPayable,
            hasPaidRecord ? payroll.Status : "Draft",
            hasPaidRecord ? payroll.PaidDate : null,
            hasPaidRecord ? payroll.RecoveryVoucher : null);
    }

    private static AttendanceEntryDto BuildDefaultAttendance(StaffProfileDto staff, DateTime date)
    {
        // Deterministic realistic daily status pattern for sample display
        var seed = (int)((staff.Id * 7 + date.Day) % 23);
        var status = date.DayOfWeek == DayOfWeek.Monday && staff.Id % 2 == 0
            ? "WeekOff"
            : seed switch
            {
                0 => "Absent",
                1 => "HalfDay",
                2 => "Leave",
                _ => "Present"
            };

        var inTime = status is "Present" or "HalfDay" ? "10:00" : "-";
        var outTime = status switch
        {
            "Present" => "22:00",
            "HalfDay" => "15:00",
            _ => "-"
        };
        var overtime = status == "Present" && (date.Day % 5 == 0) ? 2m : 0m;
        var remarks = status switch
        {
            "Present" => overtime > 0 ? "Weekend rush overtime" : "On Duty",
            "HalfDay" => "Half shift attended",
            "Leave" => "Approved leave",
            "WeekOff" => "Scheduled weekly off",
            _ => "Unplanned absence"
        };

        return new AttendanceEntryDto(
            staff.Id,
            staff.EmployeeCode,
            staff.Name,
            staff.Designation,
            staff.Shift,
            date,
            status,
            inTime,
            outTime,
            overtime,
            remarks);
    }

    private static string ToShortCode(string status) => status switch
    {
        "Present" => "P",
        "Absent" => "A",
        "HalfDay" => "HD",
        "Leave" => "L",
        "WeekOff" => "WO",
        _ => "-"
    };
}

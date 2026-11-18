using System.Globalization;
using FluentValidation;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Application.Common.Models.Accounting;
using RestaurantMS.Application.Common.Models.Staff;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Web.SampleData;

public class SampleEmployeeAdvancesUiData : IEmployeeAdvancesUiData
{
    private const string EmployeeAdvanceAccountCode = "1150";
    private const string EmployeeAdvanceAccountName = "Employee Advances A/c";

    private readonly SampleStaffStore _staffStore;
    private readonly SampleAdvanceStateStore _state;
    private readonly IAccountingService _accountingService;
    private readonly ISettingService _settingService;
    private readonly IDateTime _dateTime;
    private readonly IValidator<GiveEmployeeAdvanceRequest> _giveValidator;
    private readonly IValidator<RecordAdvanceRecoveryRequest> _recoveryValidator;

    public SampleEmployeeAdvancesUiData(
        SampleStaffStore staffStore,
        SampleAdvanceStateStore state,
        IAccountingService accountingService,
        ISettingService settingService,
        IDateTime dateTime,
        IValidator<GiveEmployeeAdvanceRequest> giveValidator,
        IValidator<RecordAdvanceRecoveryRequest> recoveryValidator)
    {
        _staffStore = staffStore;
        _state = state;
        _accountingService = accountingService;
        _settingService = settingService;
        _dateTime = dateTime;
        _giveValidator = giveValidator;
        _recoveryValidator = recoveryValidator;
        _state.EnsureSeeded(_dateTime.UtcNow);
    }

    public async Task<decimal> GetMaxAdvancePercentAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var raw = await _settingService.GetSettingAsync("MaxAdvanceSalaryPercent", "100", cancellationToken);
            if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) && parsed > 0m)
            {
                return parsed;
            }
        }
        catch
        {
            // Default to 100%
        }
        return 100m;
    }

    public Task<EmployeeAdvanceKpisDto> GetKpisAsync(CancellationToken cancellationToken = default)
    {
        lock (_state.SyncRoot)
        {
            var now = _dateTime.UtcNow;
            var totalGiven = _state.Entries.Where(e => e.EntryType == "Given").Sum(e => e.Amount);
            var recoveredThisMonth = _state.Entries
                .Where(e => e.EntryType == "Recovered" && e.Date.Month == now.Month && e.Date.Year == now.Year)
                .Sum(e => e.Amount);

            var allStaff = _staffStore.GetAll();
            var outstandingList = allStaff.Select(s => ComputeOutstandingUnsafe(s.Id)).ToList();

            return Task.FromResult(new EmployeeAdvanceKpisDto(
                totalGiven,
                recoveredThisMonth,
                outstandingList.Sum(),
                outstandingList.Count(o => o > 0m)));
        }
    }

    public Task<IReadOnlyList<EmployeeAdvanceSummaryDto>> GetEmployeeSummariesAsync(string? search = null, CancellationToken cancellationToken = default)
    {
        lock (_state.SyncRoot)
        {
            var staffList = _staffStore.GetAll();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLowerInvariant();
                staffList = staffList.Where(s =>
                    s.Name.ToLowerInvariant().Contains(term) ||
                    s.EmployeeCode.ToLowerInvariant().Contains(term) ||
                    s.Designation.ToLowerInvariant().Contains(term)).ToList();
            }

            var summaries = staffList.Select(BuildSummaryUnsafe)
                .OrderByDescending(s => s.OutstandingBalance)
                .ThenBy(s => s.EmployeeCode)
                .ToList();

            return Task.FromResult<IReadOnlyList<EmployeeAdvanceSummaryDto>>(summaries);
        }
    }

    public Task<EmployeeAdvanceSummaryDto?> GetEmployeeSummaryAsync(long staffId, CancellationToken cancellationToken = default)
    {
        lock (_state.SyncRoot)
        {
            var staff = _staffStore.GetById(staffId);
            return Task.FromResult(staff is null ? null : BuildSummaryUnsafe(staff));
        }
    }

    public Task<IReadOnlyList<AdvanceLedgerEntryDto>> GetEmployeeLedgerAsync(long staffId, CancellationToken cancellationToken = default)
    {
        lock (_state.SyncRoot)
        {
            var rows = _state.Entries
                .Where(e => e.StaffId == staffId)
                .OrderByDescending(e => e.Date)
                .ThenByDescending(e => e.Id)
                .ToList();
            return Task.FromResult<IReadOnlyList<AdvanceLedgerEntryDto>>(rows);
        }
    }

    public Task<IReadOnlyList<AdvanceLedgerEntryDto>> GetAllLedgerEntriesAsync(long? staffId = null, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        lock (_state.SyncRoot)
        {
            IEnumerable<AdvanceLedgerEntryDto> query = _state.Entries;
            if (staffId.HasValue && staffId.Value > 0)
            {
                query = query.Where(e => e.StaffId == staffId.Value);
            }
            if (fromDate.HasValue)
            {
                query = query.Where(e => e.Date.Date >= fromDate.Value.Date);
            }
            if (toDate.HasValue)
            {
                query = query.Where(e => e.Date.Date <= toDate.Value.Date);
            }

            return Task.FromResult<IReadOnlyList<AdvanceLedgerEntryDto>>(
                query.OrderByDescending(e => e.Date).ThenByDescending(e => e.Id).ToList());
        }
    }

    public Task<IReadOnlyList<MonthlyRecoveryReportRowDto>> GetMonthlyRecoveryReportAsync(int month, int year, CancellationToken cancellationToken = default)
    {
        lock (_state.SyncRoot)
        {
            var staffMap = _staffStore.GetAll().ToDictionary(s => s.Id);
            var rows = _state.Entries
                .Where(e => e.EntryType == "Recovered" && e.Date.Month == month && e.Date.Year == year)
                .OrderByDescending(e => e.Date)
                .Select(e =>
                {
                    var designation = staffMap.TryGetValue(e.StaffId, out var st) ? st.Designation : string.Empty;
                    return new MonthlyRecoveryReportRowDto(
                        e.StaffId,
                        e.EmployeeCode,
                        e.EmployeeName,
                        designation,
                        e.Date,
                        e.VoucherNumber,
                        e.PaymentMode,
                        e.Amount,
                        e.RunningBalance,
                        e.Note);
                })
                .ToList();

            return Task.FromResult<IReadOnlyList<MonthlyRecoveryReportRowDto>>(rows);
        }
    }

    public async Task<Result<AdvanceLedgerEntryDto>> GiveAdvanceAsync(GiveEmployeeAdvanceRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _giveValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.Failure<AdvanceLedgerEntryDto>(new Error("Advance.ValidationFailed", validation.Errors[0].ErrorMessage));
        }

        var staff = _staffStore.GetById(request.StaffId);
        if (staff is null)
        {
            return Result.Failure<AdvanceLedgerEntryDto>(new Error("Advance.StaffNotFound", "Selected employee was not found."));
        }

        if (!staff.IsActive)
        {
            return Result.Failure<AdvanceLedgerEntryDto>(new Error("Advance.InactiveStaff", $"Cannot give advance to inactive staff member '{staff.Name}'."));
        }

        if (await _accountingService.IsPeriodLockedAsync(request.Date, cancellationToken))
        {
            var lockDate = await _accountingService.GetLockDateAsync(cancellationToken);
            return Result.Failure<AdvanceLedgerEntryDto>(new Error(
                "Advance.PeriodLocked",
                $"Cannot post advance on {request.Date:dd MMM yyyy}: accounting period is locked up to {lockDate:dd MMM yyyy}."));
        }

        var maxPercent = await GetMaxAdvancePercentAsync(cancellationToken);
        var currentOutstanding = GetCurrentOutstanding(staff.Id);
        var projectedOutstanding = currentOutstanding + request.Amount;
        var maxAllowed = Math.Round(staff.MonthlyEquivalentSalary * (maxPercent / 100m), 2, MidpointRounding.AwayFromZero);

        if (projectedOutstanding > maxAllowed && !request.IsManagerOverride)
        {
            return Result.Failure<AdvanceLedgerEntryDto>(new Error(
                "Advance.ExceedsSalaryLimit",
                $"Total advance (₹{projectedOutstanding:N2}) exceeds {maxPercent:0.##}% of monthly salary (₹{maxAllowed:N2}). Manager override with reason is required."));
        }

        if (string.Equals(request.RecoveryPlan, "MonthlyInstalment", StringComparison.OrdinalIgnoreCase) &&
            request.MonthlyInstalmentAmount.HasValue &&
            request.MonthlyInstalmentAmount.Value > projectedOutstanding)
        {
            return Result.Failure<AdvanceLedgerEntryDto>(new Error(
                "Advance.InstalmentExceedsOutstanding",
                $"Monthly instalment (₹{request.MonthlyInstalmentAmount.Value:N2}) cannot exceed total outstanding (₹{projectedOutstanding:N2})."));
        }

        var (creditCode, creditName) = ResolvePaymentAccount(request.PaymentMode);
        var narration = $"Employee Advance to {staff.Name} ({staff.EmployeeCode}) - {request.Purpose}" +
                        (request.IsManagerOverride ? $" [Manager Override: {request.ManagerOverrideReason}]" : string.Empty);

        var voucherRequest = new PostVoucherRequest(
            request.Date,
            "Payment",
            narration,
            $"ADV-{staff.EmployeeCode}",
            [
                new AccountingVoucherLineDto(EmployeeAdvanceAccountCode, EmployeeAdvanceAccountName, request.Amount, 0m, narration),
                new AccountingVoucherLineDto(creditCode, creditName, 0m, request.Amount, $"Paid via {request.PaymentMode}")
            ]);

        var voucherResult = await _accountingService.PostVoucherAsync(voucherRequest, cancellationToken);
        if (voucherResult.IsFailure)
        {
            return Result.Failure<AdvanceLedgerEntryDto>(voucherResult.Error);
        }

        lock (_state.SyncRoot)
        {
            var newBalance = ComputeOutstandingUnsafe(staff.Id) + request.Amount;
            var startMonth = request.StartMonth ?? _dateTime.UtcNow.AddMonths(1).ToString("yyyy-MM", CultureInfo.InvariantCulture);
            var planSummary = string.Equals(request.RecoveryPlan, "MonthlyInstalment", StringComparison.OrdinalIgnoreCase)
                ? $"₹{request.MonthlyInstalmentAmount:N0}/mo from {startMonth}"
                : "One-time recovery";

            var entry = new AdvanceLedgerEntryDto(
                _state.NextEntryId++,
                staff.Id,
                staff.EmployeeCode,
                staff.Name,
                request.Date.Date,
                voucherResult.Value.VoucherNumber,
                "Given",
                request.PaymentMode,
                request.Amount,
                newBalance,
                request.Purpose,
                planSummary,
                string.IsNullOrWhiteSpace(request.Note)
                    ? (request.IsManagerOverride ? $"Manager override: {request.ManagerOverrideReason}" : request.Purpose)
                    : request.Note);

            _state.Entries.Add(entry);

            var plannedInst = string.Equals(request.RecoveryPlan, "MonthlyInstalment", StringComparison.OrdinalIgnoreCase)
                ? (request.MonthlyInstalmentAmount ?? request.Amount)
                : newBalance;

            var existingAuto = _state.RecoveryPlans.TryGetValue(staff.Id, out var existing) ? existing.AutoRecover : true;
            _state.RecoveryPlans[staff.Id] = (plannedInst, startMonth, existingAuto);

            return Result.Success(entry);
        }
    }

    public async Task<Result<AdvanceLedgerEntryDto>> RecordRecoveryAsync(RecordAdvanceRecoveryRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _recoveryValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.Failure<AdvanceLedgerEntryDto>(new Error("Advance.ValidationFailed", validation.Errors[0].ErrorMessage));
        }

        var staff = _staffStore.GetById(request.StaffId);
        if (staff is null)
        {
            return Result.Failure<AdvanceLedgerEntryDto>(new Error("Advance.StaffNotFound", "Selected employee was not found."));
        }

        if (await _accountingService.IsPeriodLockedAsync(request.Date, cancellationToken))
        {
            var lockDate = await _accountingService.GetLockDateAsync(cancellationToken);
            return Result.Failure<AdvanceLedgerEntryDto>(new Error(
                "Advance.PeriodLocked",
                $"Cannot post recovery on {request.Date:dd MMM yyyy}: accounting period is locked up to {lockDate:dd MMM yyyy}."));
        }

        var currentOutstanding = GetCurrentOutstanding(staff.Id);
        if (request.Amount > currentOutstanding)
        {
            return Result.Failure<AdvanceLedgerEntryDto>(new Error(
                "Advance.RecoveryExceedsOutstanding",
                $"Recovery amount (₹{request.Amount:N2}) cannot exceed outstanding advance balance (₹{currentOutstanding:N2})."));
        }

        var (debitCode, debitName) = ResolveRecoveryDebitAccount(request.PaymentMode);
        var voucherType = string.Equals(request.PaymentMode, "SalaryDeduction", StringComparison.OrdinalIgnoreCase)
            ? "Journal"
            : "Receipt";
        var narration = $"Advance Recovery from {staff.Name} ({staff.EmployeeCode}) via {request.PaymentMode}" +
                        (!string.IsNullOrWhiteSpace(request.Note) ? $" - {request.Note}" : string.Empty);

        var voucherRequest = new PostVoucherRequest(
            request.Date,
            voucherType,
            narration,
            $"REC-{staff.EmployeeCode}",
            [
                new AccountingVoucherLineDto(debitCode, debitName, request.Amount, 0m, $"Recovered via {request.PaymentMode}"),
                new AccountingVoucherLineDto(EmployeeAdvanceAccountCode, EmployeeAdvanceAccountName, 0m, request.Amount, narration)
            ]);

        var voucherResult = await _accountingService.PostVoucherAsync(voucherRequest, cancellationToken);
        if (voucherResult.IsFailure)
        {
            return Result.Failure<AdvanceLedgerEntryDto>(voucherResult.Error);
        }

        lock (_state.SyncRoot)
        {
            var newBalance = Math.Max(0m, ComputeOutstandingUnsafe(staff.Id) - request.Amount);
            var entry = new AdvanceLedgerEntryDto(
                _state.NextEntryId++,
                staff.Id,
                staff.EmployeeCode,
                staff.Name,
                request.Date.Date,
                voucherResult.Value.VoucherNumber,
                "Recovered",
                request.PaymentMode,
                request.Amount,
                newBalance,
                request.PaymentMode == "SalaryDeduction" ? "Salary Statement Auto-Recovery" : "Advance Recovery",
                null,
                string.IsNullOrWhiteSpace(request.Note) ? $"Recovered via {request.PaymentMode}" : request.Note);

            _state.Entries.Add(entry);

            if (newBalance <= 0m && _state.RecoveryPlans.TryGetValue(staff.Id, out var plan))
            {
                _state.RecoveryPlans[staff.Id] = (0m, null, plan.AutoRecover);
            }

            return Result.Success(entry);
        }
    }

    public Task<Result<bool>> SetAutoRecoverAsync(long staffId, bool enabled, CancellationToken cancellationToken = default)
    {
        lock (_state.SyncRoot)
        {
            var existing = _state.RecoveryPlans.TryGetValue(staffId, out var plan) ? plan : (0m, null, true);
            _state.RecoveryPlans[staffId] = (existing.Item1, existing.Item2, enabled);
            return Task.FromResult(Result.Success(enabled));
        }
    }

    private decimal GetCurrentOutstanding(long staffId)
    {
        lock (_state.SyncRoot)
        {
            return ComputeOutstandingUnsafe(staffId);
        }
    }

    private decimal ComputeOutstandingUnsafe(long staffId)
    {
        var given = _state.Entries.Where(e => e.StaffId == staffId && e.EntryType == "Given").Sum(e => e.Amount);
        var recovered = _state.Entries.Where(e => e.StaffId == staffId && (e.EntryType == "Recovered" || e.EntryType == "Adjusted")).Sum(e => e.Amount);
        return Math.Max(0m, Math.Round(given - recovered, 2, MidpointRounding.AwayFromZero));
    }

    private EmployeeAdvanceSummaryDto BuildSummaryUnsafe(StaffProfileDto staff)
    {
        var given = _state.Entries.Where(e => e.StaffId == staff.Id && e.EntryType == "Given").Sum(e => e.Amount);
        var recovered = _state.Entries.Where(e => e.StaffId == staff.Id && (e.EntryType == "Recovered" || e.EntryType == "Adjusted")).Sum(e => e.Amount);
        var outstanding = Math.Max(0m, Math.Round(given - recovered, 2, MidpointRounding.AwayFromZero));
        var plan = _state.RecoveryPlans.TryGetValue(staff.Id, out var p) ? p : (0m, null, true);

        return new EmployeeAdvanceSummaryDto(
            staff.Id,
            staff.EmployeeCode,
            staff.Name,
            staff.Designation,
            staff.PhotoUrl,
            staff.SalaryType,
            staff.BasicSalary,
            staff.MonthlyEquivalentSalary,
            staff.IsActive,
            given,
            recovered,
            outstanding,
            plan.Item1,
            plan.Item2,
            plan.Item3);
    }

    private static (string Code, string Name) ResolvePaymentAccount(string paymentMode) =>
        paymentMode.Trim().ToUpperInvariant() switch
        {
            "BANK" => ("1120", "HDFC Bank Current A/c"),
            "UPI" => ("1120", "HDFC Bank Current A/c (UPI)"),
            _ => ("1110", "Cash in Hand")
        };

    private static (string Code, string Name) ResolveRecoveryDebitAccount(string paymentMode) =>
        paymentMode.Trim().ToUpperInvariant() switch
        {
            "BANK" => ("1120", "HDFC Bank Current A/c"),
            "UPI" => ("1120", "HDFC Bank Current A/c (UPI)"),
            "SALARYDEDUCTION" => ("2130", "Staff Salaries Payable"),
            _ => ("1110", "Cash in Hand")
        };
}

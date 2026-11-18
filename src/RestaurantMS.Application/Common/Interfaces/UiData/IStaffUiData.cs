using RestaurantMS.Application.Common.Models;
using RestaurantMS.Application.Common.Models.Staff;
using RestaurantMS.Application.Common.Models.UiData;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Common.Interfaces.UiData;

public interface IStaffUiData
{
    Task<PagedResult<StaffMemberDto>> GetStaffListAsync(DataTableRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StaffMemberDto>> GetAllStaffAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StaffProfileDto>> GetStaffProfilesAsync(string? search = null, CancellationToken cancellationToken = default);
    Task<StaffProfileDto?> GetStaffProfileByIdAsync(long staffId, CancellationToken cancellationToken = default);
    Task SaveStaffMemberAsync(string name, string role, string mobile, string shift, CancellationToken cancellationToken = default);
    Task<StaffProfileDto> SaveStaffProfileAsync(StaffProfileDto profile, CancellationToken cancellationToken = default);

    // Attendance & Month Lock
    Task<IReadOnlyList<AttendanceEntryDto>> GetDailyAttendanceAsync(DateTime date, CancellationToken cancellationToken = default);
    Task<Result<bool>> SaveAttendanceEntryAsync(long staffId, DateTime date, string status, string inTime, string outTime, decimal overtimeHours, string remarks, CancellationToken cancellationToken = default);
    Task<Result<int>> MarkAllPresentAsync(DateTime date, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MonthlyCalendarDayDto>> GetMonthlyCalendarAsync(long staffId, int month, int year, CancellationToken cancellationToken = default);
    Task<bool> IsMonthLockedAsync(int month, int year, CancellationToken cancellationToken = default);
    Task<Result<bool>> SetMonthLockAsync(int month, int year, bool locked, CancellationToken cancellationToken = default);

    // Salary Statement
    Task<IReadOnlyList<SalaryStatementRowDto>> GetSalaryStatementAsync(int month, int year, bool autoRecoverAdvance, CancellationToken cancellationToken = default);
    Task<SalaryStatementRowDto?> GetPayslipAsync(long staffId, int month, int year, bool autoRecoverAdvance, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SalaryStatementRowDto>> GetStaffSalaryHistoryAsync(long staffId, CancellationToken cancellationToken = default);
    Task<Result<SalaryStatementRowDto>> MarkSalaryPaidAsync(long staffId, int month, int year, bool autoRecoverAdvance, CancellationToken cancellationToken = default);
}

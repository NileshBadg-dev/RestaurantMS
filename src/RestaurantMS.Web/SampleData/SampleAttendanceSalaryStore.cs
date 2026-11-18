using RestaurantMS.Application.Common.Models.Staff;

namespace RestaurantMS.Web.SampleData;

public class SampleAttendanceSalaryStore
{
    public object SyncRoot { get; } = new();
    public Dictionary<(long StaffId, DateTime Date), AttendanceEntryDto> AttendanceByKey { get; } = new();
    public HashSet<(int Year, int Month)> LockedMonths { get; } = new();
    public Dictionary<(long StaffId, int Year, int Month), (string Status, DateTime? PaidDate, string? RecoveryVoucher, decimal PaidRecoveryAmount)> PayrollStatus { get; } = new();
}

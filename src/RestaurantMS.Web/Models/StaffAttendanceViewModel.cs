namespace RestaurantMS.Web.Models;

public class StaffAttendanceViewModel
{
    public DateTime AttendanceDate { get; set; } = DateTime.UtcNow;
    public IReadOnlyList<StaffAttendanceRowModel> AttendanceList { get; set; } = Array.Empty<StaffAttendanceRowModel>();
    public int TotalStaff => AttendanceList.Count;
    public int TotalPresent => AttendanceList.Count(a => a.Status == "Present");
    public int TotalAbsent => AttendanceList.Count(a => a.Status == "Absent");
    public int TotalHalfDay => AttendanceList.Count(a => a.Status == "HalfDay");
    public int TotalOnLeave => AttendanceList.Count(a => a.Status == "Leave");
}

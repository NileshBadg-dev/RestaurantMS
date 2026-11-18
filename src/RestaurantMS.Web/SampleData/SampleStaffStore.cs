using RestaurantMS.Application.Common.Models.Staff;

namespace RestaurantMS.Web.SampleData;

public class SampleStaffStore
{
    private readonly object _sync = new();
    private readonly List<StaffProfileDto> _staff =
    [
        new(1, "EMP-001", "Suresh Patil", "9822114455", "Head Waiter", "Dinner (04:00 PM - 11:30 PM)", new DateTime(2024, 2, 1), "Monthly", 22000m, "Aadhaar XXXX-4812", "9822114455@okhdfc / HDFC0001234", null, true, 22000m),
        new(2, "EMP-002", "Anil Deshmukh", "9822336677", "Waiter", "Dinner (04:00 PM - 11:30 PM)", new DateTime(2024, 5, 15), "Monthly", 16000m, "Aadhaar XXXX-2219", "9822336677@ybl / SBIN0004512", null, true, 16000m),
        new(3, "EMP-003", "Meena Jadhav", "9822558899", "Captain", "Dinner (04:00 PM - 11:30 PM)", new DateTime(2023, 8, 10), "Monthly", 24000m, "PAN ABCDE1234F", "9822558899@okicici / ICIC0000812", null, true, 24000m),
        new(4, "EMP-004", "Chef Rakesh", "9822770011", "Head Chef", "All Day (10:00 AM - 10:00 PM)", new DateTime(2022, 11, 1), "Monthly", 35000m, "Aadhaar XXXX-9014", "chefrakesh@okaxis / UTIB0000123", null, true, 35000m),
        new(5, "EMP-005", "Prakash Shinde", "9822992233", "Cashier", "Evening (03:30 PM - 11:30 PM)", new DateTime(2024, 1, 20), "Monthly", 20000m, "Aadhaar XXXX-7741", "9822992233@okhdfc / HDFC0001234", null, true, 20000m),
        new(6, "EMP-006", "Kavita Pawar", "9822004466", "Kitchen Assistant", "Morning (07:00 AM - 03:30 PM)", new DateTime(2025, 3, 1), "Daily", 550m, "Aadhaar XXXX-3388", "9822004466@ybl", null, true, 16500m),
        new(7, "EMP-007", "Ganesh More", "9822441122", "Utility Helper", "Morning (07:00 AM - 03:30 PM)", new DateTime(2024, 7, 1), "Daily", 450m, "Aadhaar XXXX-6601", "Cash Payment Only", null, false, 13500m)
    ];

    public IReadOnlyList<StaffProfileDto> GetAll()
    {
        lock (_sync)
        {
            return _staff.ToList();
        }
    }

    public StaffProfileDto? GetById(long id)
    {
        lock (_sync)
        {
            return _staff.FirstOrDefault(s => s.Id == id);
        }
    }

    public StaffProfileDto Upsert(StaffProfileDto profile)
    {
        lock (_sync)
        {
            var monthlyEq = string.Equals(profile.SalaryType, "Daily", StringComparison.OrdinalIgnoreCase)
                ? profile.BasicSalary * 30m
                : profile.BasicSalary;

            if (profile.Id > 0)
            {
                var idx = _staff.FindIndex(s => s.Id == profile.Id);
                if (idx >= 0)
                {
                    var updated = profile with { MonthlyEquivalentSalary = monthlyEq };
                    _staff[idx] = updated;
                    return updated;
                }
            }

            var newId = _staff.Count > 0 ? _staff.Max(s => s.Id) + 1 : 1;
            var code = string.IsNullOrWhiteSpace(profile.EmployeeCode) ? $"EMP-{newId:D3}" : profile.EmployeeCode;
            var created = profile with { Id = newId, EmployeeCode = code, MonthlyEquivalentSalary = monthlyEq };
            _staff.Add(created);
            return created;
        }
    }
}

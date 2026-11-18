using System.Globalization;
using RestaurantMS.Application.Common.Models.Staff;

namespace RestaurantMS.Web.SampleData;

public class SampleAdvanceStateStore
{
    public object SyncRoot { get; } = new();
    public List<AdvanceLedgerEntryDto> Entries { get; } = new();
    public Dictionary<long, (decimal PlannedInstalment, string? NextMonth, bool AutoRecover)> RecoveryPlans { get; } = new();
    public long NextEntryId { get; set; } = 10;
    public bool Seeded { get; set; }

    public void EnsureSeeded(DateTime utcNow)
    {
        lock (SyncRoot)
        {
            if (Seeded) return;
            Seeded = true;

            var now = utcNow.Date;
            var monthStr = now.ToString("yyyy-MM", CultureInfo.InvariantCulture);

            Entries.AddRange(
            [
                new(1, 1, "EMP-001", "Suresh Patil", now.AddDays(-14), "PMT-2026-0101", "Given", "Cash", 5000m, 5000m, "Family Medical Emergency", $"₹2,500/mo from {monthStr}", "Paid in cash at counter"),
                new(2, 1, "EMP-001", "Suresh Patil", now.AddDays(-3), "RCP-2026-0102", "Recovered", "SalaryDeduction", 2500m, 2500m, "Monthly Salary Deduction", null, "First instalment recovered"),
                new(3, 2, "EMP-002", "Anil Deshmukh", now.AddDays(-10), "PMT-2026-0103", "Given", "UPI", 3000m, 3000m, "Festival Expenses", $"₹1,500/mo from {monthStr}", "Transferred via GPay"),
                new(4, 3, "EMP-003", "Meena Jadhav", now.AddDays(-20), "PMT-2026-0104", "Given", "Cash", 4000m, 4000m, "Travel & Commute", "One-time recovery", "Settled in full"),
                new(5, 3, "EMP-003", "Meena Jadhav", now.AddDays(-5), "RCP-2026-0105", "Recovered", "Cash", 4000m, 0m, "Cash Repayment", null, "Full advance returned"),
                new(6, 4, "EMP-004", "Chef Rakesh", now.AddDays(-8), "PMT-2026-0106", "Given", "Bank", 10000m, 10000m, "Child School Fees", $"₹4,000/mo from {monthStr}", "NEFT to HDFC account"),
                new(7, 4, "EMP-004", "Chef Rakesh", now.AddDays(-2), "RCP-2026-0107", "Recovered", "Bank", 2000m, 8000m, "Partial Repayment", null, "Direct bank transfer"),
                new(8, 5, "EMP-005", "Prakash Shinde", now.AddDays(-12), "PMT-2026-0108", "Given", "Cash", 2000m, 2000m, "Personal Advance", $"₹1,000/mo from {monthStr}", "Counter cash advance")
            ]);

            RecoveryPlans[1] = (2500m, monthStr, true);
            RecoveryPlans[2] = (1500m, monthStr, true);
            RecoveryPlans[3] = (0m, null, true);
            RecoveryPlans[4] = (4000m, monthStr, true);
            RecoveryPlans[5] = (1000m, monthStr, true);
            RecoveryPlans[6] = (0m, null, true);
            RecoveryPlans[7] = (0m, null, false);
        }
    }
}

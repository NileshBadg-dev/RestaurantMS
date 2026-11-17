namespace RestaurantMS.Web.Models;

public class ProfitLossReportViewModel
{
    public DateTime FromDate { get; set; } = DateTime.UtcNow.AddMonths(-1);
    public DateTime ToDate { get; set; } = DateTime.UtcNow;

    // Income
    public decimal DineInSales { get; set; } = 485000m;
    public decimal ParcelSales { get; set; } = 195000m;
    public decimal OnlineSales { get; set; } = 142000m;
    public decimal DiscountsGiven { get; set; } = 18500m;
    public decimal TotalNetRevenue => DineInSales + ParcelSales + OnlineSales - DiscountsGiven;

    // Direct Expenses (COGS)
    public decimal VegetablesAndDairy { get; set; } = 168000m;
    public decimal MeatAndPoultry { get; set; } = 145000m;
    public decimal GroceryAndSpices { get; set; } = 78000m;
    public decimal PackagingMaterials { get; set; } = 24000m;
    public decimal TotalCOGS => VegetablesAndDairy + MeatAndPoultry + GroceryAndSpices + PackagingMaterials;

    public decimal GrossProfit => TotalNetRevenue - TotalCOGS;
    public decimal GrossProfitMargin => TotalNetRevenue > 0 ? (GrossProfit / TotalNetRevenue) * 100 : 0;

    // Operating Expenses
    public decimal StaffSalaries { get; set; } = 140000m;
    public decimal RestaurantRent { get; set; } = 65000m;
    public decimal ElectricityAndWater { get; set; } = 28000m;
    public decimal GasAndCommercialLpg { get; set; } = 22000m;
    public decimal MaintenanceAndRepairs { get; set; } = 9500m;
    public decimal PlatformCommissions { get; set; } = 28400m;
    public decimal TotalOperatingExpenses => StaffSalaries + RestaurantRent + ElectricityAndWater + GasAndCommercialLpg + MaintenanceAndRepairs + PlatformCommissions;

    // Net Result
    public decimal NetProfit => GrossProfit - TotalOperatingExpenses;
    public decimal NetProfitMargin => TotalNetRevenue > 0 ? (NetProfit / TotalNetRevenue) * 100 : 0;
}

namespace RestaurantMS.Domain.Constants;

public static class FeatureKeys
{
    public const string KitchenDisplay = "KitchenDisplay";
    public const string Kitchen = "Kitchen.Enabled";
    public const string InventoryTracking = "InventoryTracking";
    public const string CreditUdhari = "CreditUdhari";
    public const string Credit = "Credit.Enabled";
    public const string CustomerLoyalty = "CustomerLoyalty";
    public const string OnlineOrdering = "OnlineOrdering";
    public const string TableReservation = "TableReservation";
    public const string Tables = "Tables.Enabled";
    public const string Accounts = "Accounts.Enabled";
    public const string Images = "Images.Enabled";
    public const string MultiCurrency = "MultiCurrency";
    public const string BarcodeScanning = "BarcodeScanning";
    public const string SmsNotifications = "SmsNotifications";
    public const string EmailReceipts = "EmailReceipts";
    public const string MultiBranch = "MultiBranch";
    public const string PrintKot = "PrintKot";
    public const string RoundOffBilling = "RoundOffBilling";
    public const string SplitBilling = "SplitBilling";
    public const string ServiceCharges = "ServiceCharges";

    public static IReadOnlyList<string> GetAll()
    {
        return
        [
            KitchenDisplay,
            Kitchen,
            InventoryTracking,
            CreditUdhari,
            Credit,
            CustomerLoyalty,
            OnlineOrdering,
            TableReservation,
            Tables,
            Accounts,
            Images,
            MultiCurrency,
            BarcodeScanning,
            SmsNotifications,
            EmailReceipts,
            MultiBranch,
            PrintKot,
            RoundOffBilling,
            SplitBilling,
            ServiceCharges
        ];
    }
}

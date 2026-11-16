namespace RestaurantMS.Domain.Constants;

public static class FeatureKeys
{
    public const string KitchenDisplay = "KitchenDisplay";
    public const string InventoryTracking = "InventoryTracking";
    public const string CreditUdhari = "CreditUdhari";
    public const string CustomerLoyalty = "CustomerLoyalty";
    public const string OnlineOrdering = "OnlineOrdering";
    public const string TableReservation = "TableReservation";
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
            InventoryTracking,
            CreditUdhari,
            CustomerLoyalty,
            OnlineOrdering,
            TableReservation,
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

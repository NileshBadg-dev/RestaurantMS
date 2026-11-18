using MediatR;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Settings.Common;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Settings.Queries.GetSettings;

public class GetSettingsQueryHandler : IRequestHandler<GetSettingsQuery, Result<HotelSettingsDto>>
{
    private readonly ISettingService _settingService;

    public GetSettingsQueryHandler(ISettingService settingService)
    {
        _settingService = settingService;
    }

    public async Task<Result<HotelSettingsDto>> Handle(GetSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _settingService.GetAllAsync(cancellationToken);

        var dto = new HotelSettingsDto
        {
            HotelNameEn = settings.GetValueOrDefault("HotelNameEn", "Grand Restaurant"),
            HotelNameMr = settings.GetValueOrDefault("HotelNameMr", "भव्य रेस्टॉरंट"),
            LogoUrl = settings.GetValueOrDefault("LogoUrl"),
            Address = settings.GetValueOrDefault("Address"),
            ContactNumber = settings.GetValueOrDefault("ContactNumber"),
            Email = settings.GetValueOrDefault("Email"),
            Gstin = settings.GetValueOrDefault("Gstin"),
            Fssai = settings.GetValueOrDefault("Fssai"),
            Currency = settings.GetValueOrDefault("Currency", "INR"),
            Timezone = settings.GetValueOrDefault("Timezone", "Asia/Kolkata"),
            ReceiptHeader = settings.GetValueOrDefault("ReceiptHeader"),
            ReceiptFooter = settings.GetValueOrDefault("ReceiptFooter"),
            PaperSize = settings.GetValueOrDefault("Printing.PaperSize", "80mm"),
            AutoPrintOnSettle = ParseBool(settings.GetValueOrDefault("Printing.AutoPrintOnSettle"), true),
            PrintCopies = ParseInt(settings.GetValueOrDefault("Printing.Copies"), 1),
            ShowMarathiItemNames = ParseBool(settings.GetValueOrDefault("Printing.ShowMarathiItemNames"), true),
            ShowUpiQrOnReceipt = ParseBool(settings.GetValueOrDefault("Printing.ShowUpiQrOnReceipt"), true),
            AutoPrintKot = ParseBool(settings.GetValueOrDefault("Kitchen.AutoPrintKot"), true),
            HotelUpiId = settings.GetValueOrDefault("Payments.UpiId", "grandrestaurant@okhdfc"),
            UpiReferenceRequired = ParseBool(settings.GetValueOrDefault("Payments.UpiReferenceRequired"), false),
            CardDetailsRequired = ParseBool(settings.GetValueOrDefault("Payments.CardDetailsRequired"), true)
        };

        return Result.Success(dto);
    }

    private static bool ParseBool(string? value, bool fallback) =>
        bool.TryParse(value, out var parsed) ? parsed : fallback;

    private static int ParseInt(string? value, int fallback) =>
        int.TryParse(value, out var parsed) && parsed >= 1 && parsed <= 5 ? parsed : fallback;
}

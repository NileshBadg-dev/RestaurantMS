using MediatR;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Repositories;
using RestaurantMS.Application.Settings.Common;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Settings.Commands.SaveSettings;

public class SaveSettingsCommandHandler : IRequestHandler<SaveSettingsCommand, Result<HotelSettingsDto>>
{
    private readonly ISettingRepository _settingRepository;
    private readonly ISettingService _settingService;
    private readonly IUnitOfWork _unitOfWork;

    public SaveSettingsCommandHandler(
        ISettingRepository settingRepository,
        ISettingService settingService,
        IUnitOfWork unitOfWork)
    {
        _settingRepository = settingRepository;
        _settingService = settingService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<HotelSettingsDto>> Handle(SaveSettingsCommand request, CancellationToken cancellationToken)
    {
        await _settingRepository.SetSettingAsync("HotelNameEn", request.HotelNameEn.Trim(), "Profile", cancellationToken);
        await _settingRepository.SetSettingAsync("HotelNameMr", request.HotelNameMr.Trim(), "Profile", cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.LogoUrl))
        {
            await _settingRepository.SetSettingAsync("LogoUrl", request.LogoUrl.Trim(), "Profile", cancellationToken);
        }

        await _settingRepository.SetSettingAsync("Address", request.Address?.Trim() ?? string.Empty, "Profile", cancellationToken);
        await _settingRepository.SetSettingAsync("ContactNumber", request.ContactNumber?.Trim() ?? string.Empty, "Profile", cancellationToken);
        await _settingRepository.SetSettingAsync("Email", request.Email?.Trim() ?? string.Empty, "Profile", cancellationToken);
        await _settingRepository.SetSettingAsync("Gstin", request.Gstin?.Trim().ToUpperInvariant() ?? string.Empty, "Billing", cancellationToken);
        await _settingRepository.SetSettingAsync("Fssai", request.Fssai?.Trim() ?? string.Empty, "Billing", cancellationToken);
        await _settingRepository.SetSettingAsync("Currency", request.Currency?.Trim() ?? "INR", "Billing", cancellationToken);
        await _settingRepository.SetSettingAsync("Timezone", request.Timezone?.Trim() ?? "Asia/Kolkata", "General", cancellationToken);
        await _settingRepository.SetSettingAsync("ReceiptHeader", request.ReceiptHeader?.Trim() ?? string.Empty, "Printing", cancellationToken);
        await _settingRepository.SetSettingAsync("ReceiptFooter", request.ReceiptFooter?.Trim() ?? string.Empty, "Printing", cancellationToken);
        var paperSize = string.IsNullOrWhiteSpace(request.PaperSize) ? "80mm" : request.PaperSize.Trim();
        var copies = Math.Clamp(request.PrintCopies, 1, 5);
        await _settingRepository.SetSettingAsync("Printing.PaperSize", paperSize, "Printing", cancellationToken);
        await _settingRepository.SetSettingAsync("Printing.AutoPrintOnSettle", request.AutoPrintOnSettle ? "true" : "false", "Printing", cancellationToken);
        await _settingRepository.SetSettingAsync("Printing.Copies", copies.ToString(), "Printing", cancellationToken);
        await _settingRepository.SetSettingAsync("Printing.ShowMarathiItemNames", request.ShowMarathiItemNames ? "true" : "false", "Printing", cancellationToken);
        await _settingRepository.SetSettingAsync("Printing.ShowUpiQrOnReceipt", request.ShowUpiQrOnReceipt ? "true" : "false", "Printing", cancellationToken);
        await _settingRepository.SetSettingAsync("Kitchen.AutoPrintKot", request.AutoPrintKot ? "true" : "false", "Kitchen", cancellationToken);
        await _settingRepository.SetSettingAsync("Payments.UpiId", request.HotelUpiId?.Trim() ?? string.Empty, "Payments", cancellationToken);
        await _settingRepository.SetSettingAsync("Payments.UpiReferenceRequired", request.UpiReferenceRequired ? "true" : "false", "Payments", cancellationToken);
        await _settingRepository.SetSettingAsync("Payments.CardDetailsRequired", request.CardDetailsRequired ? "true" : "false", "Payments", cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _settingService.InvalidateCache();

        var dto = new HotelSettingsDto
        {
            HotelNameEn = request.HotelNameEn.Trim(),
            HotelNameMr = request.HotelNameMr.Trim(),
            LogoUrl = request.LogoUrl,
            Address = request.Address,
            ContactNumber = request.ContactNumber,
            Email = request.Email,
            Gstin = request.Gstin,
            Fssai = request.Fssai,
            Currency = request.Currency ?? "INR",
            Timezone = request.Timezone ?? "Asia/Kolkata",
            ReceiptHeader = request.ReceiptHeader,
            ReceiptFooter = request.ReceiptFooter,
            PaperSize = paperSize,
            AutoPrintOnSettle = request.AutoPrintOnSettle,
            PrintCopies = copies,
            ShowMarathiItemNames = request.ShowMarathiItemNames,
            ShowUpiQrOnReceipt = request.ShowUpiQrOnReceipt,
            AutoPrintKot = request.AutoPrintKot,
            HotelUpiId = request.HotelUpiId?.Trim() ?? string.Empty,
            UpiReferenceRequired = request.UpiReferenceRequired,
            CardDetailsRequired = request.CardDetailsRequired
        };

        return Result.Success(dto);
    }
}

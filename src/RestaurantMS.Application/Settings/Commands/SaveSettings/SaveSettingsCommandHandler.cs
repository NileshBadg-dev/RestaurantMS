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
            ReceiptFooter = request.ReceiptFooter
        };

        return Result.Success(dto);
    }
}

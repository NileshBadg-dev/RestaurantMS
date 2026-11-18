using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Settings.Commands.SaveSettings;
using RestaurantMS.Application.Settings.Queries.GetSettings;
using RestaurantMS.Domain.Constants;
using RestaurantMS.Web.Authorization;
using RestaurantMS.Web.Models;

namespace RestaurantMS.Web.Controllers;

[Authorize]
public class SettingsController : Controller
{
    private readonly ISender _mediator;
    private readonly IFileStorage _fileStorage;

    public SettingsController(ISender mediator, IFileStorage fileStorage)
    {
        _mediator = mediator;
        _fileStorage = fileStorage;
    }

    [HttpGet]
    [PermissionAuthorize(Permissions.Settings.View)]
    public async Task<IActionResult> Index(string tab = "profile")
    {
        var result = await _mediator.Send(new GetSettingsQuery());
        var s = result.Value;

        var model = new HotelSettingsViewModel
        {
            HotelNameEn = s.HotelNameEn,
            HotelNameMr = s.HotelNameMr,
            LogoUrl = s.LogoUrl,
            Address = s.Address,
            ContactNumber = s.ContactNumber,
            Email = s.Email,
            Gstin = s.Gstin,
            Fssai = s.Fssai,
            Currency = s.Currency,
            Timezone = s.Timezone,
            ReceiptHeader = s.ReceiptHeader,
            ReceiptFooter = s.ReceiptFooter,
            PaperSize = s.PaperSize,
            AutoPrintOnSettle = s.AutoPrintOnSettle,
            PrintCopies = s.PrintCopies,
            ShowMarathiItemNames = s.ShowMarathiItemNames,
            ShowUpiQrOnReceipt = s.ShowUpiQrOnReceipt,
            AutoPrintKot = s.AutoPrintKot,
            HotelUpiId = s.HotelUpiId,
            UpiReferenceRequired = s.UpiReferenceRequired,
            CardDetailsRequired = s.CardDetailsRequired,
            ActiveTab = tab
        };

        return View(model);
    }

    [HttpPost]
    [PermissionAuthorize(Permissions.Settings.Edit)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(HotelSettingsViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string? logoUrl = model.LogoUrl;
        if (model.LogoFile is not null && model.LogoFile.Length > 0)
        {
            try
            {
                using var stream = model.LogoFile.OpenReadStream();
                logoUrl = await _fileStorage.SaveFileAsync(stream, model.LogoFile.FileName, "logos");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("LogoFile", ex.Message);
                return View(model);
            }
        }

        var command = new SaveSettingsCommand(
            model.HotelNameEn,
            model.HotelNameMr,
            logoUrl,
            model.Address,
            model.ContactNumber,
            model.Email,
            model.Gstin,
            model.Fssai,
            model.Currency,
            model.Timezone,
            model.ReceiptHeader,
            model.ReceiptFooter,
            model.PaperSize,
            model.AutoPrintOnSettle,
            model.PrintCopies,
            model.ShowMarathiItemNames,
            model.ShowUpiQrOnReceipt,
            model.AutoPrintKot,
            model.HotelUpiId,
            model.UpiReferenceRequired,
            model.CardDetailsRequired);

        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error.Description);
            return View(model);
        }

        TempData["SuccessMessage"] = "Hotel settings saved successfully.";
        return RedirectToAction(nameof(Index), new { tab = model.ActiveTab });
    }
}

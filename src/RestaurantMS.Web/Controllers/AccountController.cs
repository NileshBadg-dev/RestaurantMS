using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Localization;
using RestaurantMS.Application.Authentication.Commands.ChangePassword;
using RestaurantMS.Application.Authentication.Commands.ForgotPassword;
using RestaurantMS.Application.Authentication.Commands.Login;
using RestaurantMS.Application.Authentication.Commands.Logout;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Web.Models;

namespace RestaurantMS.Web.Controllers;

public class AccountController : Controller
{
    private readonly ISender _mediator;
    private readonly ISettingService _settingService;

    public AccountController(ISender mediator, ISettingService settingService)
    {
        _mediator = mediator;
        _settingService = settingService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        var cultureFeature = HttpContext.Features.Get<IRequestCultureFeature>();
        var isMarathi = cultureFeature?.RequestCulture.Culture.Name.StartsWith("mr", StringComparison.OrdinalIgnoreCase) == true;

        var hotelName = isMarathi
            ? await _settingService.GetSettingAsync("HotelNameMr", "भव्य रेस्टॉरंट")
            : await _settingService.GetSettingAsync("HotelNameEn", "Grand Restaurant");

        var logoUrl = await _settingService.GetSettingAsync("LogoUrl", "/images/logo.png");

        var model = new LoginViewModel
        {
            ReturnUrl = returnUrl,
            HotelName = hotelName,
            LogoUrl = logoUrl
        };

        return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var cultureFeature = HttpContext.Features.Get<IRequestCultureFeature>();
        var isMarathi = cultureFeature?.RequestCulture.Culture.Name.StartsWith("mr", StringComparison.OrdinalIgnoreCase) == true;
        model.HotelName = isMarathi
            ? await _settingService.GetSettingAsync("HotelNameMr", "भव्य रेस्टॉरंट")
            : await _settingService.GetSettingAsync("HotelNameEn", "Grand Restaurant");
        model.LogoUrl = await _settingService.GetSettingAsync("LogoUrl", "/images/logo.png");

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var command = new LoginCommand(model.UserId, model.Password, model.RememberMe);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            model.ErrorMessage = result.Error.Description;
            ModelState.AddModelError(string.Empty, result.Error.Description);
            return View(model);
        }

        var loginData = result.Value;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, loginData.UserId),
            new(ClaimTypes.Name, loginData.FullName),
            new(ClaimTypes.Role, loginData.RoleName),
            new("UserId", loginData.UserId),
            new("MustChangePassword", loginData.MustChangePassword.ToString())
        };

        foreach (var perm in loginData.Permissions)
        {
            claims.Add(new Claim("Permission", perm));
        }

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        if (loginData.MustChangePassword)
        {
            TempData["InfoMessage"] = "You must change your password before continuing.";
            return RedirectToAction(nameof(ChangePassword), new { forced = true });
        }

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        await _mediator.Send(new LogoutCommand(userId));
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [Authorize]
    public IActionResult ChangePassword(bool forced = false)
    {
        var model = new ChangePasswordViewModel
        {
            ForcedChange = forced
        };
        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction(nameof(Login));
        }

        var command = new ChangePasswordCommand(userId, model.CurrentPassword, model.NewPassword, model.ConfirmPassword);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error.Description);
            return View(model);
        }

        TempData["SuccessMessage"] = "Password changed successfully.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _mediator.Send(new ForgotPasswordCommand(model.UserId));
        model.Message = result.Value;
        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }
}

using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Models.Reports;
using RestaurantMS.Domain.Constants;
using RestaurantMS.SharedKernel;
using RestaurantMS.Web.Authorization;
using RestaurantMS.Web.Models;

namespace RestaurantMS.Web.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly IReportsService _reportsService;
    private readonly ICurrentUser _currentUser;
    private readonly ISettingService _settingService;
    private readonly IDateTime _dateTime;

    public ReportsController(
        IReportsService reportsService,
        ICurrentUser currentUser,
        ISettingService settingService,
        IDateTime dateTime)
    {
        _reportsService = reportsService;
        _currentUser = currentUser;
        _settingService = settingService;
        _dateTime = dateTime;
    }

    [HttpGet]
    [PermissionAuthorize(Permissions.Reports.View)]
    public IActionResult Index([FromQuery] string? category = "All", [FromQuery] string? search = null)
    {
        var userId = ResolveUserId();
        var catalog = _reportsService.GetCatalog(_currentUser.Permissions, IsSuperUser());
        var prefs = _reportsService.GetUserPreferences(userId);
        var favSet = new HashSet<string>(prefs.FavoriteCodes, StringComparer.OrdinalIgnoreCase);

        var viewModel = new ReportsHubViewModel
        {
            Categories = _reportsService.GetCategories(),
            AllReports = catalog,
            FavoriteReports = catalog.Where(r => favSet.Contains(r.Code)).ToList(),
            RecentReports = prefs.RecentCodes
                .Select(c => catalog.FirstOrDefault(r => r.Code.Equals(c, StringComparison.OrdinalIgnoreCase)))
                .Where(r => r is not null)
                .Cast<ReportDefinitionDto>()
                .ToList(),
            FavoriteCodes = favSet,
            SelectedCategory = string.IsNullOrWhiteSpace(category) ? "All" : category.Trim(),
            Search = search
        };

        return View(viewModel);
    }

    [HttpGet]
    [PermissionAuthorize(Permissions.Reports.View)]
    public async Task<IActionResult> ViewReport(
        [FromQuery] string? code,
        [FromQuery] bool reset = false,
        [FromQuery] string? datePreset = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] string? shift = null,
        [FromQuery] string? waiter = null,
        [FromQuery] string? table = null,
        [FromQuery] string? category = null,
        [FromQuery] string? paymentMode = null,
        [FromQuery] string? orderType = null,
        [FromQuery] string? search = null,
        [FromQuery] int? page = null,
        [FromQuery] int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        var reportCode = string.IsNullOrWhiteSpace(code) ? "RPT01" : code.Trim().ToUpperInvariant();
        var def = _reportsService.GetDefinition(reportCode);
        if (def is null)
        {
            return NotFound($"Report '{reportCode}' not found.");
        }

        if (!_reportsService.HasReportPermission(def, _currentUser.Permissions, IsSuperUser()))
        {
            return Forbid();
        }

        var userId = ResolveUserId();
        _reportsService.RecordRecentlyUsed(userId, reportCode);

        var hasExplicitQuery = !string.IsNullOrWhiteSpace(datePreset) || from.HasValue || to.HasValue ||
                               !string.IsNullOrWhiteSpace(shift) || !string.IsNullOrWhiteSpace(waiter) ||
                               !string.IsNullOrWhiteSpace(table) || !string.IsNullOrWhiteSpace(category) ||
                               !string.IsNullOrWhiteSpace(paymentMode) || !string.IsNullOrWhiteSpace(orderType) ||
                               !string.IsNullOrWhiteSpace(search) || page.HasValue || pageSize.HasValue;

        ReportFilterModel filter;
        if (reset)
        {
            _reportsService.ClearSavedUserFilter(userId, reportCode);
            filter = new ReportFilterModel { ReportCode = reportCode };
        }
        else if (hasExplicitQuery)
        {
            filter = new ReportFilterModel
            {
                ReportCode = reportCode,
                DatePreset = string.IsNullOrWhiteSpace(datePreset) ? (from.HasValue || to.HasValue ? "Custom" : "ThisMonth") : datePreset,
                FromDate = from,
                ToDate = to,
                Shift = shift ?? "All",
                Waiter = waiter ?? "All",
                Table = table ?? "All",
                Category = category ?? "All",
                PaymentMode = paymentMode ?? "All",
                OrderType = orderType ?? "All",
                Search = search,
                PageNumber = page ?? 1,
                PageSize = pageSize ?? 25
            };
            _reportsService.SaveUserFilter(userId, reportCode, filter);
        }
        else
        {
            filter = _reportsService.GetSavedUserFilter(userId, reportCode) ?? new ReportFilterModel { ReportCode = reportCode };
        }

        var exec = _reportsService.ExecuteReport(reportCode, filter);
        if (exec.IsFailure)
        {
            return NotFound(exec.Error.Description);
        }

        var prefs = _reportsService.GetUserPreferences(userId);
        var settings = await GetSafeSettingsAsync(cancellationToken);

        var viewModel = new ReportDetailViewModel
        {
            ReportCode = def.Code,
            ReportTitle = def.TitleEn,
            IsFavorite = prefs.FavoriteCodes.Contains(def.Code, StringComparer.OrdinalIgnoreCase),
            CanExport = IsSuperUser() || _currentUser.Permissions.Contains(Permissions.Reports.Export, StringComparer.OrdinalIgnoreCase),
            CanPrint = IsSuperUser() || _currentUser.Permissions.Contains(Permissions.Reports.Print, StringComparer.OrdinalIgnoreCase),
            HotelName = settings.GetValueOrDefault("HotelNameEn", "Grand Restaurant"),
            HotelAddress = settings.GetValueOrDefault("Address"),
            HotelGstin = settings.GetValueOrDefault("Gstin"),
            HotelLogoUrl = settings.GetValueOrDefault("LogoUrl"),
            Execution = exec.Value
        };

        return View(viewModel);
    }

    [HttpGet]
    [PermissionAuthorize(Permissions.Reports.View)]
    public IActionResult Data(
        [FromQuery] string code,
        [FromQuery] bool reset = false,
        [FromQuery] string? datePreset = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] string? shift = null,
        [FromQuery] string? waiter = null,
        [FromQuery] string? table = null,
        [FromQuery] string? category = null,
        [FromQuery] string? paymentMode = null,
        [FromQuery] string? orderType = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var def = _reportsService.GetDefinition(code);
        if (def is null)
        {
            return Json(Result.Failure<ReportExecutionResultDto>("Reports.NotFound", "Report not found."));
        }

        if (!_reportsService.HasReportPermission(def, _currentUser.Permissions, IsSuperUser()))
        {
            return Json(Result.Failure<ReportExecutionResultDto>("Reports.Forbidden", "You do not have permission to view this report."));
        }

        var userId = ResolveUserId();
        ReportFilterModel filter;
        if (reset)
        {
            _reportsService.ClearSavedUserFilter(userId, def.Code);
            filter = new ReportFilterModel { ReportCode = def.Code };
        }
        else
        {
            filter = new ReportFilterModel
            {
                ReportCode = def.Code,
                DatePreset = string.IsNullOrWhiteSpace(datePreset) ? "ThisMonth" : datePreset,
                FromDate = from,
                ToDate = to,
                Shift = shift ?? "All",
                Waiter = waiter ?? "All",
                Table = table ?? "All",
                Category = category ?? "All",
                PaymentMode = paymentMode ?? "All",
                OrderType = orderType ?? "All",
                Search = search,
                PageNumber = page,
                PageSize = pageSize
            };
            _reportsService.SaveUserFilter(userId, def.Code, filter);
        }

        var exec = _reportsService.ExecuteReport(def.Code, filter);
        return Json(exec);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize(Permissions.Reports.View)]
    public IActionResult ToggleFavorite([FromBody] ToggleFavoriteReportRequest? request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Code))
        {
            return Json(Result.Failure("Reports.InvalidCode", "Report code is required."));
        }

        var isFav = _reportsService.ToggleFavorite(ResolveUserId(), request.Code);
        return Json(Result.Success(new { code = request.Code.ToUpperInvariant(), isFavorite = isFav }));
    }

    [HttpGet]
    [PermissionAuthorize(Permissions.Reports.Export)]
    public async Task<IActionResult> ExportExcel(
        [FromQuery] string code,
        [FromQuery] string? datePreset = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] string? shift = null,
        [FromQuery] string? waiter = null,
        [FromQuery] string? table = null,
        [FromQuery] string? category = null,
        [FromQuery] string? paymentMode = null,
        [FromQuery] string? orderType = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var def = _reportsService.GetDefinition(code);
        if (def is null) return NotFound();
        if (!_reportsService.HasReportPermission(def, _currentUser.Permissions, IsSuperUser())) return Forbid();

        var filter = new ReportFilterModel
        {
            ReportCode = def.Code,
            DatePreset = string.IsNullOrWhiteSpace(datePreset) ? "ThisMonth" : datePreset,
            FromDate = from,
            ToDate = to,
            Shift = shift ?? "All",
            Waiter = waiter ?? "All",
            Table = table ?? "All",
            Category = category ?? "All",
            PaymentMode = paymentMode ?? "All",
            OrderType = orderType ?? "All",
            Search = search
        };

        var settings = await GetSafeSettingsAsync(cancellationToken);
        var hotelName = settings.GetValueOrDefault("HotelNameEn", "Grand Restaurant");
        var csv = _reportsService.ExportCsv(def.Code, filter, hotelName);
        if (csv.IsFailure) return NotFound(csv.Error.Description);

        var utf8Bom = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.Value)).ToArray();
        var fileName = $"{def.Code}_{_dateTime.UtcNow:yyyyMMdd_HHmm}.csv";
        return File(utf8Bom, "text/csv; charset=utf-8", fileName);
    }

    [HttpGet]
    [PermissionAuthorize(Permissions.Reports.Print)]
    public async Task<IActionResult> PrintReport(
        [FromQuery] string code,
        [FromQuery] string? datePreset = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] string? shift = null,
        [FromQuery] string? waiter = null,
        [FromQuery] string? table = null,
        [FromQuery] string? category = null,
        [FromQuery] string? paymentMode = null,
        [FromQuery] string? orderType = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var def = _reportsService.GetDefinition(code);
        if (def is null) return NotFound();
        if (!_reportsService.HasReportPermission(def, _currentUser.Permissions, IsSuperUser())) return Forbid();

        var filter = new ReportFilterModel
        {
            ReportCode = def.Code,
            DatePreset = string.IsNullOrWhiteSpace(datePreset) ? "ThisMonth" : datePreset,
            FromDate = from,
            ToDate = to,
            Shift = shift ?? "All",
            Waiter = waiter ?? "All",
            Table = table ?? "All",
            Category = category ?? "All",
            PaymentMode = paymentMode ?? "All",
            OrderType = orderType ?? "All",
            Search = search,
            PageNumber = 1,
            PageSize = 100
        };

        var exec = _reportsService.ExecuteReport(def.Code, filter);
        if (exec.IsFailure) return NotFound();

        var settings = await GetSafeSettingsAsync(cancellationToken);
        var vm = new ReportDetailViewModel
        {
            ReportCode = def.Code,
            ReportTitle = def.TitleEn,
            HotelName = settings.GetValueOrDefault("HotelNameEn", "Grand Restaurant"),
            HotelAddress = settings.GetValueOrDefault("Address", "123 Main Street, Pune, MH"),
            HotelGstin = settings.GetValueOrDefault("Gstin"),
            HotelLogoUrl = settings.GetValueOrDefault("LogoUrl"),
            Execution = exec.Value
        };

        return View("PrintReport", vm);
    }

    [HttpGet]
    [PermissionAuthorize(Permissions.Reports.Accounts)]
    public IActionResult ProfitLoss([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var viewModel = new ProfitLossReportViewModel
        {
            FromDate = from ?? _dateTime.UtcNow.AddMonths(-1),
            ToDate = to ?? _dateTime.UtcNow
        };
        return View(viewModel);
    }

    private string ResolveUserId() =>
        _currentUser.UserId ?? User.Identity?.Name ?? "admin";

    private bool IsSuperUser() =>
        _currentUser.Roles.Contains("Owner", StringComparer.OrdinalIgnoreCase) ||
        _currentUser.Roles.Contains("Admin", StringComparer.OrdinalIgnoreCase) ||
        string.Equals(_currentUser.UserId, "admin", StringComparison.OrdinalIgnoreCase) ||
        _currentUser.Roles.Count == 0;

    private async Task<IReadOnlyDictionary<string, string>> GetSafeSettingsAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _settingService.GetAllAsync(cancellationToken);
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }
}

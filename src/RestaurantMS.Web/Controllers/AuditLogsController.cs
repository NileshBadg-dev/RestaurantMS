using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Application.Common.Models;
using RestaurantMS.Web.Models;

namespace RestaurantMS.Web.Controllers;

[Authorize]
public class AuditLogsController : Controller
{
    private readonly IAuditUiData _auditUiData;
    private readonly ICurrentUser _currentUser;

    public AuditLogsController(IAuditUiData auditUiData, ICurrentUser currentUser)
    {
        _auditUiData = auditUiData;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        [FromQuery] string? user,
        [FromQuery] string? action,
        [FromQuery] string? module,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        var isSuperUser = _currentUser.Roles.Contains("Owner", StringComparer.OrdinalIgnoreCase) ||
                          _currentUser.Roles.Contains("Admin", StringComparer.OrdinalIgnoreCase) ||
                          _currentUser.Roles.Contains("Administrator", StringComparer.OrdinalIgnoreCase) ||
                          string.Equals(_currentUser.UserId, "admin", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(_currentUser.UserName, "admin", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(_currentUser.UserName, "System Administrator", StringComparison.OrdinalIgnoreCase) ||
                          _currentUser.Roles.Count == 0;

        if (!isSuperUser)
        {
            TempData["ErrorMessage"] = "Access Denied: The system activity audit log is restricted to Admin and Owner roles only.";
            return RedirectToAction("Index", "Dashboard");
        }

        var request = new DataTableRequest { Start = 0, Length = 100 };
        var pagedLogs = await _auditUiData.GetAuditLogsAsync(request, user, action, module, from, to, cancellationToken);

        var viewModel = new AuditLogsViewModel
        {
            Logs = pagedLogs.Items,
            SelectedUser = user ?? "All",
            SelectedAction = action ?? "All",
            SelectedModule = module ?? "All",
            FromDate = from,
            ToDate = to,
            TotalLogsCount = pagedLogs.TotalCount
        };

        return View(viewModel);
    }
}

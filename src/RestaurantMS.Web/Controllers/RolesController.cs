using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMS.Application.Roles.Commands.CreateRole;
using RestaurantMS.Application.Roles.Commands.SaveRolePermissionMatrix;
using RestaurantMS.Application.Roles.Commands.UpdateRole;
using RestaurantMS.Application.Roles.Queries.GetRolePermissionMatrix;
using RestaurantMS.Application.Roles.Queries.GetRoles;
using RestaurantMS.Domain.Constants;
using RestaurantMS.Web.Authorization;
using RestaurantMS.Web.Models;

namespace RestaurantMS.Web.Controllers;

[Authorize]
public class RolesController : Controller
{
    private readonly ISender _mediator;

    public RolesController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [PermissionAuthorize(Permissions.Roles.View)]
    public async Task<IActionResult> Index()
    {
        var result = await _mediator.Send(new GetRolesQuery());
        var model = new RoleListViewModel
        {
            Roles = result.Value
        };
        return View(model);
    }

    [HttpGet]
    [PermissionAuthorize(Permissions.Roles.Create)]
    public IActionResult Create()
    {
        return View(new CreateRoleViewModel());
    }

    [HttpPost]
    [PermissionAuthorize(Permissions.Roles.Create)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRoleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _mediator.Send(new CreateRoleCommand(model.Name, model.Description));
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error.Description);
            return View(model);
        }

        TempData["SuccessMessage"] = "Role created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [PermissionAuthorize(Permissions.Roles.Edit)]
    public async Task<IActionResult> Edit(long id)
    {
        var rolesResult = await _mediator.Send(new GetRolesQuery());
        var role = rolesResult.Value.FirstOrDefault(r => r.Id == id);
        if (role is null)
        {
            return NotFound();
        }

        var model = new EditRoleViewModel
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSystemRole = role.IsSystemRole
        };

        return View(model);
    }

    [HttpPost]
    [PermissionAuthorize(Permissions.Roles.Edit)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditRoleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _mediator.Send(new UpdateRoleCommand(model.Id, model.Name, model.Description));
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error.Description);
            return View(model);
        }

        TempData["SuccessMessage"] = "Role updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [PermissionAuthorize(Permissions.Roles.View)]
    public async Task<IActionResult> Matrix(long id)
    {
        var result = await _mediator.Send(new GetRolePermissionMatrixQuery(id));
        if (!result.IsSuccess)
        {
            return NotFound();
        }

        var selectedIds = new List<long>();
        foreach (var row in result.Value.Rows)
        {
            if (row.View?.IsGranted == true) selectedIds.Add(row.View.PermissionId);
            if (row.Create?.IsGranted == true) selectedIds.Add(row.Create.PermissionId);
            if (row.Edit?.IsGranted == true) selectedIds.Add(row.Edit.PermissionId);
            if (row.Delete?.IsGranted == true) selectedIds.Add(row.Delete.PermissionId);
            if (row.Print?.IsGranted == true) selectedIds.Add(row.Print.PermissionId);
            if (row.Export?.IsGranted == true) selectedIds.Add(row.Export.PermissionId);
        }

        var model = new RolePermissionMatrixViewModel
        {
            Matrix = result.Value,
            SelectedPermissionIds = selectedIds
        };

        return View(model);
    }

    [HttpPost]
    [PermissionAuthorize(Permissions.Roles.Edit)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Matrix(long roleId, List<long> selectedPermissions)
    {
        var result = await _mediator.Send(new SaveRolePermissionMatrixCommand(roleId, selectedPermissions));
        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.Error.Description;
            return RedirectToAction(nameof(Matrix), new { id = roleId });
        }

        TempData["SuccessMessage"] = "Role permissions updated successfully.";
        return RedirectToAction(nameof(Index));
    }
}

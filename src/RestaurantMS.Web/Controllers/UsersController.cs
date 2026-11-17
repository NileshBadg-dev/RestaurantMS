using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMS.Application.Roles.Queries.GetRoles;
using RestaurantMS.Application.Users.Commands.ActivateDeactivateUser;
using RestaurantMS.Application.Users.Commands.CreateUser;
using RestaurantMS.Application.Users.Commands.ResetUserPassword;
using RestaurantMS.Application.Users.Commands.UpdateUser;
using RestaurantMS.Application.Users.Queries.GetUsers;
using RestaurantMS.Domain.Constants;
using RestaurantMS.Web.Authorization;
using RestaurantMS.Web.Models;

namespace RestaurantMS.Web.Controllers;

[Authorize]
public class UsersController : Controller
{
    private readonly ISender _mediator;

    public UsersController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [PermissionAuthorize(Permissions.Users.View)]
    public async Task<IActionResult> Index(int page = 1, string? search = null)
    {
        var usersResult = await _mediator.Send(new GetUsersQuery(page, 10, search));
        var rolesResult = await _mediator.Send(new GetRolesQuery());

        var model = new UserListViewModel
        {
            Users = usersResult.Value,
            SearchTerm = search,
            Roles = rolesResult.Value
        };

        return View(model);
    }

    [HttpGet]
    [PermissionAuthorize(Permissions.Users.Create)]
    public async Task<IActionResult> Create()
    {
        var rolesResult = await _mediator.Send(new GetRolesQuery());
        var model = new CreateUserViewModel
        {
            AvailableRoles = rolesResult.Value
        };
        return View(model);
    }

    [HttpPost]
    [PermissionAuthorize(Permissions.Users.Create)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var rolesResult = await _mediator.Send(new GetRolesQuery());
            model.AvailableRoles = rolesResult.Value;
            return View(model);
        }

        var command = new CreateUserCommand(
            model.UserId,
            model.FullName,
            model.Email,
            model.PhoneNumber,
            model.Password,
            model.RoleId);

        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error.Description);
            var rolesResult = await _mediator.Send(new GetRolesQuery());
            model.AvailableRoles = rolesResult.Value;
            return View(model);
        }

        TempData["SuccessMessage"] = "User created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [PermissionAuthorize(Permissions.Users.Edit)]
    public async Task<IActionResult> Edit(long id)
    {
        var usersResult = await _mediator.Send(new GetUsersQuery(1, 100));
        var user = usersResult.Value.Items.FirstOrDefault(u => u.Id == id);
        if (user is null)
        {
            return NotFound();
        }

        var rolesResult = await _mediator.Send(new GetRolesQuery());
        var model = new EditUserViewModel
        {
            Id = user.Id,
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            RoleId = user.RoleId,
            IsActive = user.IsActive,
            AvailableRoles = rolesResult.Value
        };

        return View(model);
    }

    [HttpPost]
    [PermissionAuthorize(Permissions.Users.Edit)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var rolesResult = await _mediator.Send(new GetRolesQuery());
            model.AvailableRoles = rolesResult.Value;
            return View(model);
        }

        var command = new UpdateUserCommand(
            model.Id,
            model.FullName,
            model.Email,
            model.PhoneNumber,
            model.RoleId);

        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error.Description);
            var rolesResult = await _mediator.Send(new GetRolesQuery());
            model.AvailableRoles = rolesResult.Value;
            return View(model);
        }

        TempData["SuccessMessage"] = "User updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [PermissionAuthorize(Permissions.Users.Edit)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(long id, bool isActive)
    {
        var result = await _mediator.Send(new ActivateDeactivateUserCommand(id, isActive));
        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.Error.Description;
        }
        else
        {
            TempData["SuccessMessage"] = isActive ? "User activated." : "User deactivated.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [PermissionAuthorize(Permissions.Users.Edit)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetUserPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Password must be at least 8 characters with uppercase, lowercase, and digit.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _mediator.Send(new ResetUserPasswordCommand(model.Id, model.NewPassword));
        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.Error.Description;
        }
        else
        {
            TempData["SuccessMessage"] = $"Password reset for user {model.UserId}. User must change password on next login.";
        }

        return RedirectToAction(nameof(Index));
    }
}

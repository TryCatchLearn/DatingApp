using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AdminController : BaseApiController
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _uow;

    public AdminController(UserManager<AppUser> userManager, IUnitOfWork uow)
    {
        _userManager = userManager;
        _uow = uow;
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-with-roles")]
    public async Task<ActionResult> GetUserWithRoles()
    {
        var users = await _userManager.Users
            .OrderBy(u => u.UserName)
            .Select(u => new
            {
                u.Id,
                Username = u.UserName,
                Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
            })
            .ToListAsync();

        return Ok(users);
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("edit-roles/{username}")]
    public async Task<ActionResult> EditRoles(string username, [FromQuery]string roles)
    {
        if (string.IsNullOrEmpty(roles)) return BadRequest("You must specify a role");

        var user = await _userManager.FindByNameAsync(username);
        if (user == null) return NotFound();

        var selectedRoles = roles.Split(",");
        var userRoles = await _userManager.GetRolesAsync(user);

        var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
        if (!result.Succeeded) return BadRequest(result.Errors);

        result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
        if (!result.Succeeded) return BadRequest(result.Errors);

        return Ok(await _userManager.GetRolesAsync(user));
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photos-to-moderate")]
    public async Task<ActionResult<List<PhotoDto>>> GetPhotosToModerate()
    {
        return Ok(await _uow.PhotoRepository.GetWaitingForApproval());
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPut("approve-photo/{id}")]
    public async Task<ActionResult> ApprovePhoto(int id)
    {
        var photo = await _uow.PhotoRepository.GetById(id);
        if (photo == null) return NotFound();

        photo.IsApproved = true;
        var hasMain = await _uow.PhotoRepository.HasMain(photo.AppUserId);
        if (!hasMain)
        {
            photo.IsMain = true;
        }

        if (!await _uow.Complete()) return BadRequest("Problem approving photo");

        return Ok();
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPut("delete-photo/{id}")]
    public async Task<ActionResult<Photo>> DeletePhoto(int id)
    {
        var photo = await _uow.PhotoRepository.GetById(id);
        if (photo == null) return NotFound();
        if (photo.IsMain) return BadRequest("Main photo cannot be deleted");

        _uow.PhotoRepository.Delete(photo);
        if (!await _uow.Complete()) return BadRequest("Problem deleting photo");

        return Ok(photo);
    }
}

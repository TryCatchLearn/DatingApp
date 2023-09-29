using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class LikesController : BaseApiController
{
    private readonly IUserRepository _userRepository;
    private readonly ILikesRepository _likesRespository;

    public LikesController(IUserRepository userRepository, ILikesRepository likesRespository)
    {
        _userRepository = userRepository;
        _likesRespository = likesRespository;
    }

    [HttpPost("{username}")]
    public async Task<ActionResult> AddLike(string username)
    {
        var sourceUserId = User.GetUserId().Value;

        var likedUser = await _userRepository.GetUserByUserNameAsync(username);
        if (likedUser == null) return NotFound();

        var sourceUser = await _likesRespository.GetUserWithLikes(sourceUserId);
        if (sourceUser.UserName == username) return BadRequest("You cannot like yourself");

        var userLike = await _likesRespository.GetUserLike(sourceUserId, likedUser.Id);
        if (userLike != null) return BadRequest("You already like this user");

        userLike = new UserLike
        {
            SourceUserId = sourceUserId,
            TargetUserId = likedUser.Id
        };

        sourceUser.LikedUsers.Add(userLike);
        if (!await _userRepository.SaveAllAsync()) return BadRequest("Failed to like user");

        return Ok();
    }

    [HttpGet]
    public async Task<ActionResult<PagedList<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
    {
        likesParams.UserId = User.GetUserId().Value;

        var users = await _likesRespository.GetUserLikes(likesParams);

        Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages));

        return Ok(users);
    }
}

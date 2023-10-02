using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface ILikesRepository
{
    Task<UserLike> GetUserLike(int sourceUserId, int targetUserId);
    Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams);
    Task<AppUser> GetUserWithLikes(int userId);
}

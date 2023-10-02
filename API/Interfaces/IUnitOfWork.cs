using API.Repositories;

namespace API.Interfaces;

public interface IUnitOfWork
{
    public IUserRepository UserRepository { get; }
    public IMessageRepository MessageRepository { get; }
    public IPhotoRepository PhotoRepository { get; }
    public ILikesRepository LikesRepository { get; }
    public IPresenceRepository PresenceRepository { get; }

    Task<bool> Complete();
    bool HasChanges();
}

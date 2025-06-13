using System;

namespace API.Interfaces;

public interface IUnitOfWork
{
    IMemberRepository MemberRepository { get; }
    IMessageRepository MessageRepository { get; }
    ILikesRepository LikesRepository { get; }
    Task<bool> Complete();
    bool HasChanges();
}

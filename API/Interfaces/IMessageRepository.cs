using System;
using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface IMessageRepository
{
    void AddMessage(Message message);
    void DeleteMessage(Message message);
    Task<Message?> GetMessage(string messageId);
    Task<PaginatedResult<MessageDto>> GetMessagesForMember(MessageParams messageParams);
    Task<IReadOnlyList<MessageDto>> GetMessageThread(string currentMemberId, string recipientId);

    void AddGroup(Group group);
    Task RemoveConnection(string connectionId);
    Task<Connection?> GetConnection(string connectionId);
    Task<Group?> GetMessageGroup(string groupName);
    Task<Group?> GetGroupForConnection(string connectionId);
}

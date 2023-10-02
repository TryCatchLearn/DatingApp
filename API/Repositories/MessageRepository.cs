using API.Data;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public MessageRepository(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public void AddGroup(Group group)
    {
        _context.Groups.Add(group);
    }

    public void AddMessage(Message message)
    {
        _context.Messages.Add(message);
    }

    public void DeleteMessage(Message message)
    {
        _context.Messages.Remove(message);
    }

    public async Task<Connection> GetConnection(string connectionId)
    {
        return await _context.Connections.FindAsync(connectionId);
    }

    public async Task<Group> GetConnectionGroup(string connectionId)
    {
        return await _context.Groups
            .Include(g => g.Connections)
            .Where(g => g.Connections.Any(c => c.ConnectionId == connectionId))
            .FirstOrDefaultAsync();
    }

    public async Task<Message> GetMessage(int id)
    {
        return await _context.Messages.FindAsync(id);
    }

    public async Task<PagedList<MessageDto>> GetMessageForUser(MessageParams messageParams)
    {
        var query = _context.Messages
            .OrderByDescending(m => m.DateMessageSent)
            .AsQueryable();

        query = messageParams.Container switch
        {
            "Inbox" => query.Where(m => m.RecipientUserName == messageParams.UserName && !m.RecipientDeleted),
            "Outbox" => query.Where(m => m.SenderUserName == messageParams.UserName && !m.SenderDeleted),
            _ => query.Where(m => m.RecipientUserName == messageParams.UserName &&
                !m.RecipientDeleted && m.DateRead == null)
        };

        var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

        return await PagedList<MessageDto>
            .CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
    }

    public async Task<Group> GetMessageGroup(string name)
    {
        return await _context.Groups
            .Include(g => g.Connections)
            .FirstOrDefaultAsync(g => g.Name == name);
    }

    public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUserName)
    {
        var query = _context.Messages
            .Where(m => (m.RecipientUserName == currentUserName && !m.RecipientDeleted &&
                         m.SenderUserName == recipientUserName) ||
                        (m.RecipientUserName == recipientUserName && !m.SenderDeleted &&
                         m.SenderUserName == currentUserName))
            .OrderBy(m => m.DateMessageSent);

        await query
            .Where(m => m.DateRead == null && m.RecipientUserName == currentUserName)
            .ForEachAsync(m => m.DateRead = DateTime.UtcNow);

        return await query
            .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public void RemoveConnection(Connection connection)
    {
        _context.Connections.Remove(connection);
    }

    public async Task<bool> SaveAllAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}

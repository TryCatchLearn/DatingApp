using API.Data;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Repository;

public class MessageRepository : IMessageRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public MessageRepository(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public void AddMessage(Message message)
    {
        _context.Messages.Add(message);
    }

    public void DeleteMessage(Message message)
    {
        _context.Messages.Remove(message);
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

    public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUserName)
    {
        var messages = await _context.Messages
            .Include(m => m.Sender).ThenInclude(s => s.Photos)
            .Where(m => (m.RecipientUserName == currentUserName && !m.RecipientDeleted &&
                         m.SenderUserName == recipientUserName) ||
                        (m.RecipientUserName == recipientUserName && !m.SenderDeleted &&
                         m.SenderUserName == currentUserName))
            .OrderBy(m => m.DateMessageSent)
            .ToListAsync();
        var unreadMessages = messages.Where(
            m => m.DateRead == null &&
            m.RecipientUserName == currentUserName).ToList();

        if (unreadMessages.Any())
        {
            foreach (var unreadMessage in unreadMessages)
            {
                unreadMessage.DateRead = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        return _mapper.Map<IEnumerable<MessageDto>>(messages);
    }

    public async Task<bool> SaveAllAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}

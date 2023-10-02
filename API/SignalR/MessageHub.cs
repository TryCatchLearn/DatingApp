using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize]
public class MessageHub : Hub
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IHubContext<PresenceHub> _presenceHub;

    public MessageHub(IUnitOfWork uow,
                      IMapper mapper,
                      IHubContext<PresenceHub> presenceHub)
    {
        _uow = uow;
        _mapper = mapper;
        _presenceHub = presenceHub;
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();

        var otherUser = httpContext.Request.Query["user"];
        var thisUser = Context.User.GetUserName();

        var groupName = GetGroupName(thisUser, otherUser);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        var group = await AddToGroup(groupName);
        await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

        var messages = await _uow.MessageRepository.GetMessageThread(thisUser, otherUser);
        if (_uow.HasChanges()) await _uow.Complete();

        await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var group = await RemoveFromGroup();
        await Clients.Group(group.Name).SendAsync("UpdatedGroup");

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
        var username = Context.User.GetUserName();
        if (username == createMessageDto.RecipientUserName.ToLower())
            throw new HubException("You cannot send a message to yourslef");

        var sender = await _uow.UserRepository.GetUserByUserNameAsync(username);
        var recipient = await _uow.UserRepository.GetUserByUserNameAsync(createMessageDto.RecipientUserName)
            ?? throw new Exception("Recipient not found");
        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUserName = sender.UserName,
            RecipientUserName = recipient.UserName,
            Content = createMessageDto.Content
        };

        var groupName = GetGroupName(sender.UserName, recipient.UserName);
        var group = await _uow.MessageRepository.GetMessageGroup(groupName);
        if (group.Connections.Any(c => c.UserName == recipient.UserName))
        {
            message.DateRead = DateTime.UtcNow;
        }
        else
        {
            var connections = await _uow.PresenceRepository.GetPresenceByUserName(recipient.UserName);
            if (connections != null)
            {
                await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived",
                    new { username = sender.UserName, knownAs = sender.KnownAs });
            }
        }

        _uow.MessageRepository.AddMessage(message);

        if (!await _uow.Complete()) throw new Exception("Failed to send message");

        await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
    }

    private string GetGroupName(string caller, string other)
    {
        return string.CompareOrdinal(caller, other) < 0
            ? $"{caller}-{other}"
            : $"{other}-{caller}";
    }

    private async Task<Group> AddToGroup(string name)
    {
        var group = await _uow.MessageRepository.GetMessageGroup(name);
        var connection = new Connection(Context.ConnectionId, Context.User.GetUserName());

        if (group == null)
        {
            group = new Group(name);
            _uow.MessageRepository.AddGroup(group);
        }
        group.Connections.Add(connection);

        if (!await _uow.Complete()) throw new Exception("Failed to add connection to group");

        return group;
    }

    private async Task<Group> RemoveFromGroup()
    {
        var group = await _uow.MessageRepository.GetConnectionGroup(Context.ConnectionId);
        var connection = group.Connections.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
        _uow.MessageRepository.RemoveConnection(connection);

        if (!await _uow.Complete()) throw new Exception("Failed to remove connection from group");

        return group;
    }
}

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
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IHubContext<PresenceHub> _presenceHub;
    private readonly IPresenceRepository _presenceRepository;

    public MessageHub(IMessageRepository messageRepository,
                      IUserRepository userRepository,
                      IMapper mapper,
                      IHubContext<PresenceHub> presenceHub,
                      IPresenceRepository presenceRepository)
    {
        _messageRepository = messageRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _presenceHub = presenceHub;
        _presenceRepository = presenceRepository;
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

        var messages = await _messageRepository.GetMessageThread(thisUser, otherUser);
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

        var sender = await _userRepository.GetUserByUserNameAsync(username);
        var recipient = await _userRepository.GetUserByUserNameAsync(createMessageDto.RecipientUserName)
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
        var group = await _messageRepository.GetMessageGroup(groupName);
        if (group.Connections.Any(c => c.UserName == recipient.UserName))
        {
            message.DateRead = DateTime.UtcNow;
        }
        else
        {
            var connections = await _presenceRepository.GetPresenceByUserName(recipient.UserName);
            if (connections != null)
            {
                await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived",
                    new { username = sender.UserName, knownAs = sender.KnownAs });
            }
        }

        _messageRepository.AddMessage(message);

        if (!await _messageRepository.SaveAllAsync()) throw new Exception("Failed to send message");

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
        var group = await _messageRepository.GetMessageGroup(name);
        var connection = new Connection(Context.ConnectionId, Context.User.GetUserName());

        if (group == null)
        {
            group = new Group(name);
            _messageRepository.AddGroup(group);
        }
        group.Connections.Add(connection);

        if (!await _messageRepository.SaveAllAsync()) throw new Exception("Failed to add connection to group");

        return group;
    }

    private async Task<Group> RemoveFromGroup()
    {
        var group = await _messageRepository.GetConnectionGroup(Context.ConnectionId);
        var connection = group.Connections.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
        _messageRepository.RemoveConnection(connection);

        if (!await _messageRepository.SaveAllAsync()) throw new Exception("Failed to remove connection from group");

        return group;
    }
}

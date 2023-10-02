using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize]
public class PresenceHub : Hub
{
    private readonly IPresenceRepository _presenceRepository;

    public PresenceHub(IPresenceRepository presenceRepository)
    {
        _presenceRepository = presenceRepository;
    }

    public override async Task OnConnectedAsync()
    {
        var username = Context.User.GetUserName();

        var isOnline = await _presenceRepository.AddPresence(username, Context.ConnectionId);
        if (isOnline) await Clients.Others.SendAsync("UserIsOnline", username);

        var usersOnline = await _presenceRepository.GetAllPresence();
        await Clients.Others.SendAsync("UsersOnline", usersOnline);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var username = Context.User.GetUserName();

        var isOffline = await _presenceRepository.DeletePresence(username, Context.ConnectionId);
        if (isOffline) await Clients.Others.SendAsync("UserIsOffline", username);

        await base.OnDisconnectedAsync(exception);
    }
}

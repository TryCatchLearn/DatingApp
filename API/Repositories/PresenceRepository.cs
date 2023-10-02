using API.Interfaces;

namespace API.Repositories;

public class PresenceRepository : IPresenceRepository
{
    private static readonly Dictionary<string, List<string>> _OnLineUsers = new();

    public Task<IEnumerable<string>> GetAllPresence()
    {
        lock (_OnLineUsers)
        {
            return Task.FromResult(_OnLineUsers.OrderBy(o => o.Key)
                .Select(o => o.Key));
        }
    }

    public Task<List<string>> GetPresenceByUserName(string username)
    {
        List<string> connectionIds;
        lock (_OnLineUsers)
        {
            connectionIds = _OnLineUsers.GetValueOrDefault(username);
        }

        return Task.FromResult(connectionIds);
    }

    public Task<bool> AddPresence(string username, string connectionId)
    {
        bool isOnline = false;
        lock(_OnLineUsers)
        {
            if (_OnLineUsers.ContainsKey(username))
            {
                _OnLineUsers[username].Add(connectionId);
            }
            else
            {
                _OnLineUsers.Add(username, new List<string>{ connectionId });
                isOnline = true;
            }
        }

        return Task.FromResult(isOnline);
    }

    public Task<bool> DeletePresence(string username, string connectionId)
    {
        bool isOffline = false;
        if (!_OnLineUsers.ContainsKey(username)) return Task.FromResult(isOffline);

        lock(_OnLineUsers)
        {
            _OnLineUsers[username].Remove(connectionId);
            if (_OnLineUsers[username].Count == 0)
            {
                _OnLineUsers.Remove(username);
                isOffline = true;
            }
        }

        return Task.FromResult(isOffline);
    }
}

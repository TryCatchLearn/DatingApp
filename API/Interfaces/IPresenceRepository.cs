namespace API.Interfaces;

public interface IPresenceRepository
{
    public Task<IEnumerable<string>> GetAllPresence();
    public Task<List<string>> GetPresenceByUserName(string username);
    public Task<bool> AddPresence(string username, string connectionId);
    public Task<bool> DeletePresence(string username, string connectionId);
}

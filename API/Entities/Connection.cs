using System;

namespace API.Entities;

public class Connection(string connectionId, string userId)
{
    public string ConnectionId { get; set; } = connectionId;
    public string UserId { get; set; } = userId;

    // nav property
    public Group Group { get; set; } = null!;
}

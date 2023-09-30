namespace API.DTOs;

public class MessageDto
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public string SenderUserName { get; set; }
    public string SenderPhotoUrl { get; set; }
    public int RecipientId { get; set; }
    public string RecipientUserName { get; set; }
    public string RecipientPhotoUrl { get; set; }
    public string Content { get; set; }
    public DateTime? DateRead { get; set; }
    public DateTime? DateMessageSent { get; set; }
}

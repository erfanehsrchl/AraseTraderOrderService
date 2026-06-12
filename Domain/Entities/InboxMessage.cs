namespace Domain.Entities;

public class InboxMessage
{
    public long Id { get; set; }
    public Guid MessageId { get; set; }
    public string MessageType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

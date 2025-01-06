namespace AISmart.Application.Grains.Event.Dto;

public class SubscriptionStatusDto
{
    public string SubscriptionId { get; set; }
    public string SubscriberId { get; set; }
    public List<string> EventTypes { get; set; }
    public string CallbackUrl { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
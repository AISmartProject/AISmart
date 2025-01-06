namespace AISmart.Application.Grains.Event.Dto;

public class SubscribeEventInputDto
{
    public string AgentId { get; set; }
    public List<string> EventTypes { get; set; }
    public string CallbackUrl { get; set; }
}

public class SubscriptionDto
{
    public string SubscriptionId { get; set; }
    public string AgentId { get; set; }
    public List<string> EventTypes { get; set; }
    public string CallbackUrl { get; set; }
    public string Status { get; set; } // active
    public DateTime CreatedAt { get; set; }
}
namespace AISmart.Application.Grains.Agents.Messaging;

public class MessagingGState : StateBase
{
    public int ReceivedMessages { get; set; } = 0;
    
    public void Apply(MessagingGEvent message)
    {
        ReceivedMessages++;
    }
}
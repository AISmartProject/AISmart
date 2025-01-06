namespace AISmart.GAgent.AtomicAgent.Models;

[GenerateSerializer]
public class TelegramAgentData
{
    public string TelegramId { get; set; }
    public string TelegramKey { get; set; }
    public List<TelegramAbility> TelegramAbilities { get; set; }
}
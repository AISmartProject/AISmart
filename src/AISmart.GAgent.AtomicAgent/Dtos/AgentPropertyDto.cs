using AISmart.GAgent.AtomicAgent.Models;

namespace AISmart.GAgent.AtomicAgent.Dtos;

public class AgentPropertyDto
{
    // AI Basic Data
    public string? ModelProvider { get; set; }
    public string? Bio { get; set; }
    public string? Lore { get; set; }
    public string? Topic { get; set; }
    public List<string>? KnowledgeBase { get; set; }

    // Telegram Messaging Data
    public string? TelegramId { get; set; }
    public string? TelegramKey { get; set; }
    public List<TelegramAbility>? TelegramAbilities { get; set; }

    // Twitter Messaging Data
    public string? TwitterId { get; set; }
    public string? TwitterKey { get; set; }
    public List<TwitterAbility>? TwitterAbilities { get; set; }
}
namespace AISmart.GAgent.AtomicAgent.Models;

[GenerateSerializer]
public class TwitterAgentData
{
    public string TwitterId { get; set; }
    public string TwitterKey { get; set; }
    public List<TwitterAbility> TwitterAbilities { get; set; }
}
using System.Threading.Tasks;
using AISmart.Agents;
using Orleans;

namespace AISmart.Sender;

public interface IPublishingAgent : IAgent
{
    Task PublishEventAsync<T>(T @event) where T : GEvent;
}
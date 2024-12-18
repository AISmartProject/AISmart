using System.Threading.Tasks;
using AISmart.Agents;
using Orleans;

namespace AISmart.Sender;

public interface IPublishingGAgent : IGAgent
{
    Task PublishEventAsync<T>(T @event) where T : EventBase;
}
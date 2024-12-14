using System.Collections.Generic;
using System.Threading.Tasks;
using AISmart.Agents;

namespace AISmart.Sender;

public interface IPublishingAgent : IAgent
{
    Task PublishEventAsync<T>(T @event) where T : EventBase;
    Task PublishEventAsync<T>(List<T> events) where T : EventBase;
}
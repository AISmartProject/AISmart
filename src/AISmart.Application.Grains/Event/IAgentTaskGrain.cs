using AISmart.Domain.Grains.Event;

namespace AISmart.Application.Grains.Event;

using Orleans;
using System.Threading.Tasks;

public interface IAgentTaskGrain : IGrainWithGuidKey
{
    Task<List<CreatedAgentEvent>> CreateTask(Guid templateId, string param);
    
    Task<List<CreatedAgentEvent>> CompleteEvent(Guid eventId,bool isSuccess, string failReason = null, string result = null);
    Task<AgentTaskDto> GetTask();
}
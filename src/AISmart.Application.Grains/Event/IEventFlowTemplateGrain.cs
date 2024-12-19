using AISmart.Domain.Grains.Event;

namespace AISmart.Application.Grains.Event;

public interface IEventFlowTemplateGrain : IGrainWithGuidKey
{
    public Task CreateEventNode(EventFlowTemplateDto eventFlowTemplateDto);
    
    
    public Task<EventFlowTemplateDto> GetEventNode();

}
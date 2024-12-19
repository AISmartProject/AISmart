using System.Linq.Expressions;
using AISmart.Agents;
using AISmart.Application.Grains.Agents.Group;
using AISmart.Application.Grains.Agents.Publisher;
using AISmart.Sender;
using Orleans.TestKit;

namespace AISmart.Grains.Tests;

public abstract class GAgentTestKitBase : TestKitBase
{
    protected async Task<PublishingGAgent> CreatePublishingGAgentAsync(params IGAgent[] gAgentsToPublish)
    {
        var publishingGAgent = await Silo.CreateGrainAsync<PublishingGAgent>(Guid.NewGuid());
        Silo.AddProbe<IPublishingGAgent>(_ => publishingGAgent);
        foreach (var gAgent in gAgentsToPublish)
        {
            await publishingGAgent.PublishTo(gAgent);
        }

        return publishingGAgent;
    }

    protected async Task<GroupGAgent> CreateGroupGAgentAsync(params IGAgent[] gAgentsToRegister)
    {
        var groupGAgent = await Silo.CreateGrainAsync<GroupGAgent>(Guid.NewGuid());
        foreach (var gAgent in gAgentsToRegister)
        {
            await groupGAgent.Register(gAgent);
        }

        await groupGAgent.Register(groupGAgent);
        return groupGAgent;
    }

    protected void AddProbes(params IGAgent?[] gAgents)
    {
        var parameter = Expression.Parameter(typeof(IdSpan), "idSpan");
        Expression body = Expression.Constant(null, typeof(IGAgent));

        foreach (var gAgent in gAgents)
        {
            var primaryKey = gAgent.GetPrimaryKey();
            var grainId = GrainIdKeyExtensions.CreateGuidKey(primaryKey);
            var condition = Expression.Equal(parameter, Expression.Constant(grainId));
            var result = Expression.Constant(gAgent, typeof(IGAgent));
            body = Expression.Condition(condition, result, body);
        }

        var lambda = Expression.Lambda<Func<IdSpan, IGAgent>>(body, parameter).Compile();
        Silo.AddProbe(lambda);
    }
}
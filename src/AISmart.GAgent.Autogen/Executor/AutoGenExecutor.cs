using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using AISmart.Agents;
using AISmart.Dapr;
using AISmart.GAgent.Autogen.Common;
using AISmart.GAgent.Autogen.Event;
using AISmart.GAgent.Autogen.Events;
using AISmart.GAgent.Autogen.Exceptions;
using AISmart.GEvents.Autogen;
using AISmart.Sender;
using AutoGen.Core;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Orleans.Streams;

namespace AISmart.GAgent.Autogen;

public class AutoGenExecutor : Grain, IAutoGenExecutor
{
    private IStreamProvider StreamProvider => this.GetStreamProvider(CommonConstants.StreamProvider);
    private readonly AgentDescriptionManager _agentDescriptionManager;
    private readonly IChatAgentProvider _chatAgentProvider;
    private readonly IGrainFactory _clusterClient;
    private readonly ILogger<AutoGenExecutor> _logger;
    private readonly Guid _publishGrainId = Guid.NewGuid();
    private const string AgentName = "admin";
    private const string FinishFlag = "complete";
    private const string BreakFlag = "break";

    public AutoGenExecutor(ILogger<AutoGenExecutor> logger, IGrainFactory clusterClient,
        AgentDescriptionManager agentDescriptionManager, IChatAgentProvider chatAgentProvider)
    {
        _logger = logger;
        _clusterClient = clusterClient;
        _agentDescriptionManager = agentDescriptionManager;
        _chatAgentProvider = chatAgentProvider;
    }

    public async Task ExecuteTaskAsync(ExecutorTaskInfo taskInfo)
    {
        var history = ConvertMessage(taskInfo.History);
        var taskId = taskInfo.TaskId;
        _chatAgentProvider.SetAgent(AgentName, GetAgentResponsibility(), GetMiddleware());
        var response = await _chatAgentProvider.SendAsync(AgentName, "What should be done next?", history);
        var responseStr = response.GetContent();
        if (responseStr.IsNullOrEmpty())
        {
            _logger.LogDebug(
                $"[AutoGenExecutor] autoGen response is null, History:{JsonSerializer.Serialize(history)}");
            return;
        }

        if (responseStr.Contains(FinishFlag))
        {
            var completeEvent = JsonSerializer.Deserialize<EventComplete>(responseStr);
            if (completeEvent != null)
            {
                // var publishGrain = _clusterClient.GetGrain<IPublishingGAgent>(_publishGrainId);
                await PublishInternalEvent(new AutoGenExecutorEvent()
                {
                    TaskId = taskId,
                    ExecuteStatus = TaskExecuteStatus.Finish,
                    EndContent = completeEvent.Complete,
                });
                return;
            }

            _logger.LogDebug(
                $"[AutoGenExecutor] response is finished,but Deserialize error, History:{JsonSerializer.Serialize(history)} response:{responseStr}");
        }
        else if (responseStr.Contains(BreakFlag))
        {
            var breakInfo = JsonSerializer.Deserialize<EventBreak>(responseStr);
            if (breakInfo != null)
            {
                // var publishGrain = _clusterClient.GetGrain<IPublishingGAgent>(_publishGrainId);
                await PublishInternalEvent(new AutoGenExecutorEvent()
                {
                    TaskId = taskId,
                    ExecuteStatus = TaskExecuteStatus.Break,
                    EndContent = breakInfo.Break,
                });
                return;
            }

            _logger.LogDebug(
                $"[AutoGenExecutor] response is break,but Deserialize error, History:{JsonSerializer.Serialize(history)} response:{responseStr}");
        }

        var handleEventSchema = JsonSerializer.Deserialize<HandleEventAsyncSchema>(responseStr);
        if (handleEventSchema != null)
        {
            // var publishGrain = _clusterClient.GetGrain<IPublishingGAgent>(_publishGrainId);
            await PublishInternalEvent(new AutoGenExecutorEvent()
            {
                TaskId = taskId,
                ExecuteStatus = TaskExecuteStatus.Progressing,
                CurrentCallInfo = responseStr
            });
        }
    }

    private List<IMessage> ConvertMessage(List<AutogenMessage> listAutoGenMessage)
    {
        var result = new List<IMessage>();
        foreach (var item in listAutoGenMessage)
        {
            result.Add(new TextMessage(GetRole(item.Role), item.Content));
        }

        return result;
    }

    private Role GetRole(string roleName)
    {
        switch (roleName)
        {
            case "user":
                return Role.User;
            case "assistant":
                return Role.Assistant;
            case "system":
                return Role.System;
            case "function":
                return Role.Function;
            default:
                return Role.User;
        }
    }

    private FunctionCallMiddleware GetMiddleware()
    {
        var groupChatContract = new List<FunctionContract>()
        {
            HandleEventAsyncContract
        };

        var groupChatFunctionMap = new Dictionary<string, Func<string, Task<string>>>()
        {
            { nameof(HandleEventAsync), HandleEventAsyncWrapper }
        };

        return new FunctionCallMiddleware(groupChatContract, groupChatFunctionMap);
    }

    private string GetAgentResponsibility()
    {
        return $$"""
                 You are a manager who solves user problems by organizing agents,
                 - The following is a JSON-formatted description of all proxies, including the events each proxy can handle and the parameters for each event:
                 {{_agentDescriptionManager.GetAutoGenEventDescription()}}

                 - You can understand what the event can do through the event description, and know the type, name, and description of each parameter through the event parameters. 
                 If the event description can handle the user's request, you need to assemble the request into the following JSON format:
                 {
                     AgentName:"",
                     EventName:"",
                     Parameters:""
                 }
                 JSON explanation:
                 "AgentName" is the name of the proxy to be called. 
                 "EventName" is the name of the event to be invoked on the proxy, 
                 "Parameters" are the parameters for the event. The parameter types and names must match the defined ones.

                 The workflow is as follows:
                 - You take the problem from user.
                 - Split the task into different events, and use the output of the previous event combined with the user's request to determine the execution of the next event.
                 - Based on the event's response, reorganize the next request in line with the user's intent until the user's issue is resolved.
                 - If the above events cannot meet the user's task execution needs, you can generate results based on the events to drive the continuation of the process.
                 - The response for each event will be added to the conversation in JSON format. 
                   You need to analyze the response information to decide whether to proceed to the next round. 
                   The response information will be used during the final summary. The JSON format is as follows:
                   {
                    "EventName":"", 
                    "Reply":"",
                   }
                   JSON explanation:EventName is the name of the event,and Reply is the response from the event corresponding to EventName.
                   
                 - If the user's request is completed, please output the Json format:
                    ```
                    {
                        "complete":"{reply summary}"
                    }
                    ```
                    
                 - If the user's request cannot continue, please output the Json format:
                    ```
                    {
                        "break": "{question}"
                    }
                    ```
                 """;
    }

    private async Task PublishInternalEvent(AutoGenInternalEventBase publishData)
    {
        var streamId = StreamId.Create(CommonConstants.StreamNamespace, this.GetPrimaryKey());
        var stream = StreamProvider.GetStream<AutoGenInternalEventBase>(streamId);
        await stream.OnNextAsync(publishData);
    }
    
    #region Event hook

    /// <summary>
    /// handle middleware event
    /// </summary>
    /// <param name="agentName">agent name</param>
    /// <param name="eventName">event name</param>
    /// <param name="parameters">event parameter json string</param>
    /// <returns></returns>
    [Function]
    public async Task<string> HandleEventAsync(string agentName, string eventName, string parameters)
    {
        var descriptionDic = _agentDescriptionManager.GetAgentDescription();
        if (descriptionDic.TryGetValue(agentName, out var eventDescription) == false)
        {
            throw new AutogenException($"Event name:{agentName} not exist");
        }

        var eventInfo = eventDescription.EventList.FirstOrDefault(f => f.EventName == eventName);
        if (eventInfo == null)
        {
            throw new AutogenException(
                $"agentName:{agentName} Event name:{eventName} Deserialize object is null, the parameters is{parameters}");
        }

        var eventData = JsonSerializer.Deserialize(parameters, eventInfo.EventType) as EventBase;
        if (eventData == null)
        {
            throw new AutogenException(
                $"Event name:{agentName} Deserialize object is null, the parameters is{parameters}");
        }

        // var publishGrain = _clusterClient.GetGrain<IPublishingGAgent>(_publishGrainId);
        await PublishInternalEvent(new PassThroughExecutorEvent() { PassThroughData = eventData });

        return JsonSerializer.Serialize(new HandleEventAsyncSchema()
            { AgentName = agentName, EventName = eventName, Parameters = parameters });
    }

    public FunctionContract HandleEventAsyncContract
    {
        get => new FunctionContract
        {
            ClassName = nameof(AutogenGAgent),
            Name = nameof(HandleEventAsync),
            Description = @"Handle agent's event",
            ReturnType = typeof(Task<string>),
            ReturnDescription = "event handle result",
            Parameters = new global::AutoGen.Core.FunctionParameterContract[]
            {
                new FunctionParameterContract
                {
                    Name = @"agentName",
                    Description = @"agent name",
                    ParameterType = typeof(string),
                    IsRequired = true,
                },
                new FunctionParameterContract
                {
                    Name = @"eventName",
                    Description = @"event name",
                    ParameterType = typeof(string),
                    IsRequired = true,
                },
                new FunctionParameterContract()
                {
                    Name = @"parameters",
                    Description = @"event parameters",
                    ParameterType = typeof(string),
                    IsRequired = true,
                }
            },
        };
    }

    public async Task<string> HandleEventAsyncWrapper(string arguments)
    {
        var schema = JsonSerializer.Deserialize<HandleEventAsyncSchema>(
            arguments,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            });

        string result;
        try
        {
            result = await HandleEventAsync(schema.AgentName, schema.EventName, schema.Parameters);
        }
        catch (AutogenException e)
        {
            result = JsonSerializer.Serialize(new EventBreak() { Break = e.Message });
        }

        return result;
    }

    public class HandleEventAsyncSchema
    {
        [JsonPropertyName(@"agentName")] public string AgentName { get; set; }
        [JsonPropertyName(@"eventName")] public string EventName { get; set; }
        [JsonPropertyName(@"parameters")] public string Parameters { get; set; }
    }

    public class EventBreak
    {
        [JsonPropertyName(@"break")] public string Break { get; set; }
    }

    public class EventComplete
    {
        [JsonPropertyName(@"compete")] public string Complete { get; set; }
    }

    #endregion
}
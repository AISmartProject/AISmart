using System.Reflection;
using AISmart.Agents;
using Microsoft.Extensions.Logging;

namespace AISmart.GAgent.Core;

public abstract partial class GAgentBase<TState, TEvent>
{
    private Task UpdateObserverList()
    {
        var eventHandlerMethods = GetEventHandlerMethods();

        foreach (var eventHandlerMethod in eventHandlerMethods)
        {
            var observer = new EventWrapperBaseAsyncObserver(async item =>
            {
                var grainId = (GrainId)item.GetType().GetProperty(nameof(EventWrapper<object>.GrainId))?.GetValue(item)!;
                if (grainId == this.GetGrainId())
                {
                    // Skip the event if it is sent by itself.
                    return;
                }

                var eventId = (Guid)item.GetType().GetProperty(nameof(EventWrapper<object>.EventId))?.GetValue(item)!;
                var eventType = item.GetType().GetProperty(nameof(EventWrapper<object>.Event))?.GetValue(item);
                var parameter = eventHandlerMethod.GetParameters()[0];

                var contextStorageGrainIdValue = item.GetType()
                    .GetProperty(nameof(EventWrapper<object>.ContextGrainId))?
                    .GetValue(item);
                if (contextStorageGrainIdValue != null)
                {
                    var contextStorageGrainId = (GrainId)contextStorageGrainIdValue;
                    var contextStorageGrain = GrainFactory.GetGrain<IContextStorageGrain>(contextStorageGrainId.GetGuidKey());
                    if (contextStorageGrain != null)
                    {
                        var context = await contextStorageGrain.GetContext();
                        (eventType! as EventBase)!.SetContext(context);
                    }
                }
                
                var originStreamIdValue = item.GetType()
                    .GetProperty(nameof(EventWrapper<EventBase>.RootStreamIdList))?
                    .GetValue(item);
                var originStreamId = new List<StreamId>();
                if (originStreamIdValue != null)
                {
                    originStreamId = (List<StreamId>)originStreamIdValue;
                    (eventType! as EventBase)!.SetRootStreamIdList(originStreamId);
                }

                if (parameter.ParameterType == eventType!.GetType())
                {
                    await HandleMethodInvocationAsync(eventHandlerMethod, parameter, eventType, eventId,
                        originStreamId);
                }

                if (parameter.ParameterType == typeof(EventWrapperBase))
                {
                    try
                    {
                        var invokeParameter =
                            new EventWrapper<EventBase>((EventBase)eventType, eventId, this.GetGrainId(),
                                originStreamId);
                        var result = eventHandlerMethod.Invoke(this, [invokeParameter]);
                        await (Task)result!;
                    }
                    catch (Exception ex)
                    {
                        // TODO: Make this better.
                        Logger.LogError(ex, "Error invoking method {MethodName} with event type {EventType}",
                            eventHandlerMethod.Name, eventType.GetType().Name);
                    }
                }
            });

            Observers.Add(observer, new Dictionary<StreamId, Guid>());
        }

        return Task.CompletedTask;
    }

    private IEnumerable<MethodInfo> GetEventHandlerMethods()
    {
        return GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .Where(IsEventHandlerMethod);
    }

    private bool IsEventHandlerMethod(MethodInfo methodInfo)
    {
        return methodInfo.GetParameters().Length == 1 && (
            // Either the method has the EventHandlerAttribute
            // Or is named HandleEventAsync
            //     and the parameter is not EventWrapperBase 
            //     and the parameter is inherited from EventBase
            ((methodInfo.GetCustomAttribute<EventHandlerAttribute>() != null ||
              methodInfo.Name == AISmartGAgentConstants.EventHandlerDefaultMethodName) &&
             methodInfo.GetParameters()[0].ParameterType != typeof(EventWrapperBase) &&
             typeof(EventBase).IsAssignableFrom(methodInfo.GetParameters()[0].ParameterType))
            // Or the method has the AllEventHandlerAttribute and the parameter is EventWrapperBase
            || (methodInfo.GetCustomAttribute<AllEventHandlerAttribute>() != null &&
                methodInfo.GetParameters()[0].ParameterType == typeof(EventWrapperBase)));
    }

    private async Task HandleMethodInvocationAsync(MethodInfo method, ParameterInfo parameter, object eventType,
        Guid eventId, List<StreamId> originStreamId)
    {
        if (IsEventWithResponse(parameter))
        {
            await HandleEventWithResponseAsync(method, eventType, eventId, originStreamId);
        }
        else if (method.ReturnType == typeof(Task))
        {
            try
            {
                var result = method.Invoke(this, [eventType]);
                await (Task)result!;
            }
            catch (Exception ex)
            {
                // TODO: Make this better.
                Logger.LogError(ex, "Error invoking method {MethodName} with event type {EventType}", method.Name,
                    eventType.GetType().Name);
            }
        }
    }

    private bool IsEventWithResponse(ParameterInfo parameter)
    {
        return parameter.ParameterType.BaseType is { IsGenericType: true } &&
               parameter.ParameterType.BaseType.GetGenericTypeDefinition() == typeof(EventWithResponseBase<>);
    }

    private async Task HandleEventWithResponseAsync(MethodInfo method, object eventType, Guid eventId,
        List<StreamId> originStreamId)
    {
        if (method.ReturnType.IsGenericType &&
            method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var resultType = method.ReturnType.GetGenericArguments()[0];
            if (typeof(EventBase).IsAssignableFrom(resultType))
            {
                try
                {
                    var eventResult = await (dynamic)method.Invoke(this, [eventType])!;
                    var eventWrapper =
                        new EventWrapper<EventBase>(eventResult, eventId, this.GetGrainId(), originStreamId);
                    await SendEventToRootAsync(eventWrapper);
                }
                catch (Exception ex)
                {
                    // TODO: Make this better.
                    Logger.LogError(ex, "Error invoking method {MethodName} with event type {EventType}", method.Name,
                        eventType.GetType().Name);
                }
            }
            else
            {
                var errorMessage =
                    $"The event handler of {eventType.GetType()}'s return type needs to be inherited from EventBase.";
                Logger.LogError(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }
        }
        else
        {
            var errorMessage =
                $"The event handler of {eventType.GetType()} needs to have a return value.";
            Logger.LogError(errorMessage);
            throw new InvalidOperationException(errorMessage);
        }
    }
}
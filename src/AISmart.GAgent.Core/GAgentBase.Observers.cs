using System.Reflection;
using AElf.OpenTelemetry.ExecutionTime;
using AISmart.Agents;
using AISmart.Dapr;
using Microsoft.Extensions.Logging;

namespace AISmart.GAgent.Core;

public abstract partial class GAgentBase<TState, TEvent>
{
    private async Task UpdateObserverList()
    {
        Logger.LogInformation($"{this.GetGrainId().ToString()}: UpdateObserverList");

        var eventHandlerMethods = GetEventHandlerMethods().ToList();

        Logger.LogInformation($"Found {eventHandlerMethods.Count} event handler methods on {this.GetGrainId().ToString()}.");

        foreach (var eventHandlerMethod in eventHandlerMethods)
        {
            var observer = new EventWrapperBaseAsyncObserver(async item =>
            {
                var grainId = (GrainId)item.GetType().GetProperty(nameof(EventWrapper<EventBase>.GrainId))?.GetValue(item)!;
                if (grainId == this.GetGrainId() && eventHandlerMethod.Name != nameof(ForwardEventAsync))
                {
                    // Skip the event if it is sent by itself.
                    return;
                }

                var eventId = (Guid)item.GetType().GetProperty(nameof(EventWrapper<EventBase>.EventId))?.GetValue(item)!;
                var eventType = (EventBase)item.GetType().GetProperty(nameof(EventWrapper<EventBase>.Event))?.GetValue(item)!;
                var parameter = eventHandlerMethod.GetParameters()[0];

                Logger.LogInformation($"Handling event {eventType.GetType().Name} with method {eventHandlerMethod.Name}");

                _correlationId = (Guid?)item.GetType().GetProperty(nameof(EventWrapper<EventBase>.CorrelationId))
                    ?.GetValue(item);
                // if (_correlationId != null &&
                //     StreamId.Create(CommonConstants.StreamNamespace, this.GetGrainId().ToString()) != streamId)
                // {
                //     //_streamIdDictionary.TryAdd(_correlationId.Value, streamId);
                // }

                if (parameter.ParameterType == eventType.GetType())
                {
                    await HandleMethodInvocationAsync(eventHandlerMethod, parameter, eventType, eventId);
                }

                if (parameter.ParameterType == typeof(EventWrapperBase))
                {
                    try
                    {
                        var invokeParameter =
                            new EventWrapper<EventBase>(eventType, eventId, this.GetGrainId())
                            {
                                CorrelationId = _correlationId
                            };
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

            observer.MethodName = eventHandlerMethod.Name;
            observer.ParameterTypeName = eventHandlerMethod.GetParameters()[0].ParameterType.Name;
            _observers.Add(observer, Guid.Empty);
        }

        Logger.LogInformation($"Added {_observers.Count} event handlers to {this.GetGrainId().ToString()}.");
    }

    private async Task UpdateObserverListAgain()
    {
        Logger.LogInformation($"{this.GetGrainId().ToString()}: UpdateObserverListAgain");
        await UpdateObserverList();
    }

    private IEnumerable<MethodInfo> GetEventHandlerMethods()
    {
        Logger.LogInformation($"{this.GetGrainId().ToString()}: GetEventHandlerMethods, {GetType().FullName}");
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

    private async Task HandleMethodInvocationAsync(MethodInfo method, ParameterInfo parameter, EventBase eventType,
        Guid eventId)
    {
        if (IsEventWithResponse(parameter))
        {
            await HandleEventWithResponseAsync(method, eventType, eventId);
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

    private async Task HandleEventWithResponseAsync(MethodInfo method, EventBase eventType, Guid eventId)
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
                    eventResult.CorrelationId = _correlationId;
                    var eventWrapper =
                        new EventWrapper<EventBase>(eventResult, eventId, this.GetGrainId());
                    await PublishAsync(eventWrapper);
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
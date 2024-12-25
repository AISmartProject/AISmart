using System.Linq.Expressions;
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
                var grainId =
                    (GrainId)item.GetType().GetProperty(nameof(EventWrapper<EventBase>.GrainId))?.GetValue(item)!;
                if (grainId == this.GetGrainId())
                {
                    // Skip the event if it is sent by itself.
                    return;
                }

                var eventId = (Guid)item.GetType().GetProperty(nameof(EventWrapper<EventBase>.EventId))?.GetValue(item)!;
                var eventType = (EventBase)item.GetType().GetProperty(nameof(EventWrapper<EventBase>.Event))
                    ?.GetValue(item)!;
                var parameter = eventHandlerMethod.GetParameters()[0];

                var contextStorageGrainIdValue = item.GetType()
                    .GetProperty(nameof(EventWrapper<EventBase>.ContextGrainId))?
                    .GetValue(item);
                if (contextStorageGrainIdValue != null)
                {
                    var contextStorageGrainId = (GrainId)contextStorageGrainIdValue;
                    var contextStorageGrain =
                        GrainFactory.GetGrain<IContextStorageGrain>(contextStorageGrainId.GetGuidKey());
                    if (contextStorageGrain != null)
                    {
                        var context = await contextStorageGrain.GetContext();
                        eventType.SetContext(context);
                    }
                }

                if (parameter.ParameterType == eventType.GetType())
                {
                    await HandleMethodInvocationAsync(eventHandlerMethod, parameter, eventType, eventId);
                }

                if (parameter.ParameterType == typeof(EventWrapperBase))
                {
                    try
                    {
                        var invokeParameter =
                            new EventWrapper<EventBase>(eventType, eventId, this.GetGrainId());
                        var instance = Expression.Constant(this);
                        var methodCall = Expression.Call(instance, eventHandlerMethod,
                            Expression.Constant(invokeParameter));
                        var lambda = Expression.Lambda<Func<Task>>(methodCall).Compile();
                        await lambda();
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
        var methodInfoParam = Expression.Parameter(typeof(MethodInfo), "methodInfo");

        // methodInfo.GetParameters().Length == 1
        var getParametersCall = Expression.Call(methodInfoParam, nameof(MethodInfo.GetParameters), Type.EmptyTypes);
        var lengthProperty = Expression.Property(getParametersCall, nameof(Array.Length));
        var lengthCheck = Expression.Equal(lengthProperty, Expression.Constant(1));

        // methodInfo.GetCustomAttribute<EventHandlerAttribute>() != null
        var getCustomAttributeCall = Expression.Call(
            typeof(CustomAttributeExtensions),
            nameof(CustomAttributeExtensions.GetCustomAttribute),
            new[] { typeof(EventHandlerAttribute) },
            methodInfoParam
        );
        var customAttributeCheck = Expression.NotEqual(getCustomAttributeCall,
            Expression.Constant(null, typeof(EventHandlerAttribute)));

        // methodInfo.Name == AISmartGAgentConstants.EventHandlerDefaultMethodName
        var nameProperty = Expression.Property(methodInfoParam, nameof(MethodInfo.Name));
        var nameCheck = Expression.Equal(nameProperty,
            Expression.Constant(AISmartGAgentConstants.EventHandlerDefaultMethodName));

        // methodInfo.GetParameters()[0].ParameterType != typeof(EventWrapperBase)
        var firstParameter = Expression.ArrayIndex(getParametersCall, Expression.Constant(0));
        var parameterTypeProperty = Expression.Property(firstParameter, nameof(ParameterInfo.ParameterType));
        var parameterTypeCheck =
            Expression.NotEqual(parameterTypeProperty, Expression.Constant(typeof(EventWrapperBase)));

        // typeof(EventBase).IsAssignableFrom(methodInfo.GetParameters()[0].ParameterType)
        var eventBaseType = Expression.Constant(typeof(EventBase), typeof(Type));
        var isAssignableFromCall = Expression.Call(
            eventBaseType,
            nameof(Type.IsAssignableFrom),
            Type.EmptyTypes,
            parameterTypeProperty
        );

        // methodInfo.GetCustomAttribute<AllEventHandlerAttribute>() != null
        var getAllCustomAttributeCall = Expression.Call(
            typeof(CustomAttributeExtensions),
            nameof(CustomAttributeExtensions.GetCustomAttribute),
            new[] { typeof(AllEventHandlerAttribute) },
            methodInfoParam
        );
        var allCustomAttributeCheck = Expression.NotEqual(getAllCustomAttributeCall,
            Expression.Constant(null, typeof(AllEventHandlerAttribute)));

        // methodInfo.GetParameters()[0].ParameterType == typeof(EventWrapperBase)
        var allParameterTypeCheck =
            Expression.Equal(parameterTypeProperty, Expression.Constant(typeof(EventWrapperBase)));

        // Combine all conditions
        var combinedCondition = Expression.OrElse(
            Expression.AndAlso(
                lengthCheck,
                Expression.OrElse(
                    Expression.AndAlso(
                        Expression.OrElse(customAttributeCheck, nameCheck),
                        Expression.AndAlso(parameterTypeCheck, isAssignableFromCall)
                    ),
                    Expression.AndAlso(allCustomAttributeCheck, allParameterTypeCheck)
                )
            ),
            Expression.AndAlso(allCustomAttributeCheck, allParameterTypeCheck)
        );

        var lambda = Expression.Lambda<Func<MethodInfo, bool>>(combinedCondition, methodInfoParam).Compile();
        return lambda(methodInfo);
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
                var instance = Expression.Constant(this);
                var methodCall = Expression.Call(instance, method, Expression.Constant(eventType));
                var lambda = Expression.Lambda<Func<Task>>(methodCall).Compile();
                await lambda();
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
        var eventHandler = CreateEventHandlerDelegate(method, eventType.GetType());

        if (eventHandler != null)
        {
            var eventResult = await eventHandler(eventType);
            var eventWrapper = new EventWrapper<EventBase>(eventResult, eventId, this.GetGrainId());
            await PublishAsync(eventWrapper);
        }
        else
        {
            var errorMessage =
                $"The event handler of {eventType.GetType()}'s return type needs to be inherited from EventBase.";
            Logger.LogError(errorMessage);
            throw new InvalidOperationException(errorMessage);
        }
    }

    private Func<EventBase, Task<EventBase>>? CreateEventHandlerDelegate(MethodInfo method, Type eventType)
    {
        var instance = Expression.Constant(this);
        var parameter = Expression.Parameter(typeof(EventBase), "eventType");
        var castParameter = Expression.Convert(parameter, eventType);
        var call = Expression.Call(instance, method, castParameter);

        if (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var resultType = method.ReturnType.GetGenericArguments()[0];
            if (typeof(EventBase).IsAssignableFrom(resultType))
            {
                var convertCall = Expression.Call(
                    typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(typeof(EventBase)),
                    Expression.Convert(Expression.Property(call, nameof(Task<object>.Result)), typeof(EventBase))
                );
                var lambda = Expression.Lambda<Func<EventBase, Task<EventBase>>>(convertCall, parameter);
                return lambda.Compile();
            }
        }

        return null;
    }
}
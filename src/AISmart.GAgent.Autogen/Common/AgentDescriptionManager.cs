using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using AISmart.Agents;
using Volo.Abp.DependencyInjection;
using AISmart.GAgent.Autogen.Exceptions;
using AutoGen.Core;
using Newtonsoft.Json;

namespace AISmart.GAgent.Autogen.Common;

public class AgentDescriptionManager : ISingletonDependency
{
    private readonly Dictionary<string, AgentDescriptionInfo> _agentDescription =
        new Dictionary<string, AgentDescriptionInfo>();

    private string _autoGenAgentEventDescription = string.Empty;

    public AgentDescriptionManager()
    {
        // _agentDescription = GetAllAgentDescription();
        // _autoGenAgentEventDescription = AssembleAllAgentDescription();
    }

    public void AddAgentEvents(Type agentType, List<Type> eventTypes)
    {
        if (_agentDescription.TryGetValue(agentType.Name, out var agentDescription) == false)
        {
            agentDescription = new AgentDescriptionInfo();
            agentDescription.AgentName = agentType.Name;
            var description = agentType.GetCustomAttribute<DescriptionAttribute>();
            if (description == null)
            {
                throw new AutogenException($"agent:{agentType.Name} does not contain DescriptionAttribute");
            }

            agentDescription.AgentDescription = description.Description;

            _agentDescription.Add(agentType.Name, agentDescription);
        }

        foreach (var eventType in eventTypes)
        {
            agentDescription.EventList.Add(GetEventDescription(agentType.Name, eventType));
        }

        _autoGenAgentEventDescription = AssembleAllAgentDescription();
    }

    public ReadOnlyDictionary<string, AgentDescriptionInfo> GetAgentDescription()
    {
        return new ReadOnlyDictionary<string, AgentDescriptionInfo>(_agentDescription);
    }

    public string GetAutoGenEventDescription()
    {
        return _autoGenAgentEventDescription;
    }

    private AgentEventDescription GetEventDescription(string agentName, Type eventType)
    {
        var result = new AgentEventDescription();
        result.EventName = eventType.Name;
        var description = eventType.GetCustomAttribute<DescriptionAttribute>();
        if (description == null)
        {
            throw new AutogenException($"agentName:{agentName} event:{eventType.Name} does not contain DescriptionAttribute");
        }

        result.EventDescription = description.Description;
        result.EventType = eventType;
        var fields = eventType.GetProperties();
        foreach (var field in fields)
        {
            result.EventParameters.Add(GetEventTypeDescription(agentName, eventType.Name, field));
        }

        return result;
    }

    private AgentEventTypeFieldDescription GetEventTypeDescription(string agentName, string eventName,
        PropertyInfo fieldType)
    {
        var descriptionAttributes = fieldType.GetCustomAttributes(typeof(DescriptionAttribute), false);
        if (descriptionAttributes == null)
        {
            throw new AutogenException(
                $"agentName:{agentName} eventName:{eventName} field:{fieldType.Name} description not exist");
        }

        foreach (var description in descriptionAttributes)
        {
            if (description is DescriptionAttribute)
            {
                var fieldDescription = new AgentEventTypeFieldDescription();
                fieldDescription.FieldName = fieldType.Name;
                fieldDescription.FieldDescription = (description as DescriptionAttribute).Description;
                fieldDescription.FieldType = fieldType.PropertyType.Name;

                return fieldDescription;
            }
        }

        return null;
    }

    private Dictionary<string, AgentDescriptionInfo> GetAllAgentDescription()
    {
        var result = new Dictionary<string, AgentDescriptionInfo>();

        string currentDirectory = Directory.GetCurrentDirectory();
        string[] dllFiles = Directory.GetFiles(currentDirectory, "*.dll");
        foreach (var dllFilePath in dllFiles)
        {
            try
            {
                Assembly assembly = Assembly.LoadFrom(dllFilePath);
                if (assembly == null)
                {
                    continue;
                }

                var dllAgentDescription = GenerateAgentDescription(assembly);
                foreach (var description in dllAgentDescription)
                {
                    if (result.ContainsKey(description.Key))
                    {
                        throw new AutogenException($"Agent name:{description.Key} has exist");
                    }

                    result.Add(description.Key, description.Value);
                }
            }
            catch (Exception e)
            {
                continue;
            }
        }

        return result;
    }

    private Dictionary<string, AgentDescriptionInfo> GenerateAgentDescription(Assembly assembly)
    {
        var result = new Dictionary<string, AgentDescriptionInfo>();
        try
        {
            var agentList = assembly.GetTypes();

            foreach (var classType in agentList)
            {
                if (!classType.IsClass || classType.GetInterface(nameof(IAgent)) == null)
                {
                    continue;
                }

                var agentDescription = new AgentDescriptionInfo();
                var description = classType.GetCustomAttribute<DescriptionAttribute>();
                if (description == null)
                {
                    continue;
                }

                agentDescription.AgentName = classType.Name;
                agentDescription.AgentDescription = description.Description;
                //
                // var agentGenericList = classType.GetGenericArguments();
                // if (agentGenericList.Length != 2)
                // {
                //     throw new Exception($"ClassName:{classType.Name} generic error");
                // }
                //
                // var eventType = agentGenericList[1];
                //
                // // get all field name
                // FieldInfo[] fields = eventType.GetFields(BindingFlags.Public);
                // var fieldList = new List<AgentEventTypeFieldDescription>();
                // foreach (var fieldType in fields)
                // {
                //     var descriptionAttribute = fieldType.GetCustomAttribute<DescriptionAttribute>();
                //     if (descriptionAttribute == null)
                //     {
                //         throw new Exception($"ClassName:{eventType.Name} field:{fieldType.Name} description not exsit");
                //     }
                //
                //     var fieldDescription = new AgentEventTypeFieldDescription();
                //     fieldDescription.FieldName = fieldType.Name;
                //     fieldDescription.FieldDescription = descriptionAttribute.Description;
                //     fieldDescription.FieldType = fieldType.FieldType;
                //     fieldList.Add(fieldDescription);
                // }
                //
                // agentDescription.EventParameters = fieldList;

                result.Add(agentDescription.AgentName, agentDescription);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("ex");
        }

        return result;
    }

    private string AssembleAllAgentDescription()
    {
        var availableAgent = _agentDescription.Values.Where(s => s.EventList.Count > 0).ToList();
        if (availableAgent.Count > 0)
        {
            var agentsDescription = JsonConvert.SerializeObject(availableAgent);
            return agentsDescription;
        }

        return "{}";
    }

    private string AssembleEventDescription(AgentEventDescription eventDescription)
    {
        var result = new StringBuilder();

        StringBuilder agentDescription = new StringBuilder();
        agentDescription.AppendLine("{");
        agentDescription.AppendLine($"  EventName:\"{eventDescription.EventName}\",");
        agentDescription.AppendLine($"  EventDescription:\"{eventDescription.EventDescription}\",");
        agentDescription.AppendLine($"  EventParameters:[");
        agentDescription.AppendLine($"{AssembleAgentEventParameterDescription(eventDescription.EventParameters)}");
        agentDescription.AppendLine($"  ]");
        agentDescription.AppendLine("}");
        result.Append(agentDescription.ToString());

        return result.ToString();
    }

    private string AssembleAgentEventParameterDescription(List<AgentEventTypeFieldDescription> fieldDescriptionList)
    {
        var result = new StringBuilder();
        var firstFlag = fieldDescriptionList.Count > 1;
        foreach (var item in fieldDescriptionList)
        {
            var fieldDescription = new StringBuilder();
            fieldDescription.AppendLine("   {");
            fieldDescription.AppendLine($"      ParameterType:\"{nameof(item.FieldType)}\",");
            fieldDescription.AppendLine($"      ParameterName:\"{item.FieldName}\",");
            fieldDescription.AppendLine($"      ParameterDescription:\"{item.FieldDescription}\",");
            fieldDescription.AppendLine($"      ParameterIsRequired:true");
            if (firstFlag)
            {
                fieldDescription.AppendLine("   },");
                firstFlag = false;
            }
            else
            {
                fieldDescription.AppendLine("   }");
            }

            result.AppendLine(fieldDescription.ToString());
        }

        return result.ToString();
    }
}
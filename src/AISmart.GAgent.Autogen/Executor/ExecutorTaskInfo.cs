using AISmart.GAgent.Autogen.Common;
using AutoGen.Core;

namespace AISmart.GAgent.Autogen;

[GenerateSerializer]
public class ExecutorTaskInfo
{
   [Id(0)] public Guid TaskId { get; set; }
   [Id(1)] public List<AutogenMessage> History { get; set; }
}
using AutoGen.Core;

namespace AISmart.GAgent.Autogen;

public interface IAutoGenExecutor : IGrainWithGuidKey
{
    // Task ExecuteTaskAsync(Guid taskId, List<IMessage> history);
    Task ExecuteTaskAsync(ExecutorTaskInfo taskInfo);
}
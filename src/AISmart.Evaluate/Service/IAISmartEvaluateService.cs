using System.Threading.Tasks;

namespace AISmart.Evaluate.Service;

public interface IAISmartEvaluateService
{
    Task EvaluateAsync(string task, string result);
    Task AddExceptionMessageAsync(string task, string exceptionMessage);
    Task<string> GetAdviceAsync(string task);
}
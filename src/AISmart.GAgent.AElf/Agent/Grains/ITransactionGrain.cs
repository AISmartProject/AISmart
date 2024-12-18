using System.Threading.Tasks;
using AISmart.Agent.Event;
using AISmart.Dto;
using Orleans;

namespace AISmart.Agent.Grains;

public interface ITransactionGrain:IGrainWithGuidKey
{
    Task <TransactionDto> SendAElfTransactionAsync(SendTransactionDto sendTransactionDto);
    Task <TransactionDto> LoadAElfTransactionResultAsync(QueryTransactionDto queryTransactionDto);
    
    Task <TransactionDto> GetAElfTransactionAsync(QueryTransactionDto queryTransactionDto);
}
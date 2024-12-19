using System.Threading.Tasks;
using AISmart.CQRS.Dto;

namespace AISmart.CQRS;

public interface IIndexingService
{
    public void CheckExistOrCreateIndex(string typeName);
    public Task SaveOrUpdateIndexAsync(string typeName,BaseStateIndex baseStateIndex);
    
    public Task<BaseStateIndex> QueryIndexAsync(string id,string indexName);

}
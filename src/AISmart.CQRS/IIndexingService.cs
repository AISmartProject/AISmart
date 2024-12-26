using System.Threading.Tasks;
using AISmart.Agents;
using AISmart.CQRS.Dto;
using Nest;

namespace AISmart.CQRS;

public interface IIndexingService
{
    public void CheckExistOrCreateStateIndex(string typeName);
    public Task SaveOrUpdateStateIndexAsync(string typeName,BaseStateIndex baseStateIndex);
    
    public Task<BaseStateIndex> QueryStateIndexAsync(string id,string indexName);

    public void CheckExistOrCreateGEventIndex<T>(T gEvent) where T : GEventBase;
    
    public Task SaveOrUpdateGEventIndexAsync<T>(T gEvent) where T : GEventBase;

    public Task QueryEventIndexAsync<T>(string id) where T : GEventBase;

    public Task<ISearchResponse<dynamic>> QueryEventIndexV2Async(string id, string indexName);


}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AISmart.Agents;
using AISmart.CQRS.Dto;

namespace AISmart.CQRS;

public interface IIndexingService
{
    public void CheckExistOrCreateStateIndex(string typeName);
    public Task SaveOrUpdateStateIndexAsync(string typeName,BaseStateIndex baseStateIndex);
    
    public Task<BaseStateIndex> QueryStateIndexAsync(string id,string indexName);

    public void CheckExistOrCreateGEventIndex<T>(T gEvent) where T : GEventBase;
    
    public Task SaveOrUpdateGEventIndexAsync<T>(T gEvent) where T : GEventBase;

    public Task<string> QueryEventIndexAsync(string id, string indexName);
    public Task<string> QueryEventIndexAsync(DateTime beginDateTime,DateTime endDateTime, string indexName);

    public void CheckExistOrCreateIndex<T>() where T : class;
    public Task SaveOrUpdateChatLogIndexAsync(AIChatLogIndex index);
    
    public Task<(long TotalCount,List<AIChatLogIndex> ChatLogs)> QueryChatLogListAsync(ChatLogQueryInputDto input);

}
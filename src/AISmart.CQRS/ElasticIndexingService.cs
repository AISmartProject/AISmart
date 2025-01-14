using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AISmart.Agents;
using AISmart.CQRS.Dto;
using Microsoft.Extensions.Logging;
using Nest;
using Newtonsoft.Json;
namespace AISmart.CQRS;

public class ElasticIndexingService : IIndexingService
{
    private readonly IElasticClient _elasticClient;
    private readonly ILogger<ElasticIndexingService> _logger;
    private const string IndexSuffix = "index";
    private const string CTime = "CTime";

    public ElasticIndexingService(ILogger<ElasticIndexingService> logger, IElasticClient elasticClient)
    {
        _logger = logger;
        _elasticClient = elasticClient;
    }

    public void CheckExistOrCreateStateIndex(string typeName)
    {
        var indexName = typeName.ToLower() + IndexSuffix;
        var indexExistsResponse = _elasticClient.Indices.Exists(indexName);
        if (indexExistsResponse.Exists)
        {
            return;
        }
        var createIndexResponse = _elasticClient.Indices.Create(indexName, c => c
            .Map<BaseStateIndex>(m => m.AutoMap())
        );
        if (!createIndexResponse.IsValid)
        {
            _logger.LogError("Error creating state index {indexName} {error} {createIndexResponse}", indexName , createIndexResponse.ServerError?.Error, JsonConvert.SerializeObject(createIndexResponse));
        }
        else
        {
            _logger.LogInformation("Index created state index successfully. {indexName}", indexName);
        }
    }

    public async Task SaveOrUpdateStateIndexAsync(string typeName, BaseStateIndex baseStateIndex)
    {
        var indexName = typeName.ToLower() + IndexSuffix;
        await _elasticClient.IndexAsync(baseStateIndex, i => i
            .Index(indexName)
            .Id(baseStateIndex.Id)
        );
    }

    public async Task<BaseStateIndex> QueryStateIndexAsync(string id,string indexName)
    {
        var response = await _elasticClient.GetAsync<BaseStateIndex>(id, g => g.Index(indexName));
        return response.Source; 
    }

    public void CheckExistOrCreateGEventIndex<T>(T gEvent) where T : GEventBase
    {
        var indexName = gEvent.GetType().Name.ToLower() + IndexSuffix;
        var indexExistsResponse = _elasticClient.Indices.Exists(indexName);
        if (indexExistsResponse.Exists)
        {
            return;
        }

        var createIndexResponse = _elasticClient.Indices.Create(indexName, c => c
            .Map<T>(m => m
                .AutoMap()
                .Properties(props =>
                {
                    var type = gEvent.GetType();
                    foreach (var property in type.GetProperties())
                    {
                        var propertyName = property.Name;
                        if (property.PropertyType == typeof(string))
                        {
                            props.Keyword(k => k
                                .Name(propertyName)
                            );
                        }
                        else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(long))
                        {
                            props.Number(n => n
                                .Name(propertyName)
                                .Type(NumberType.Long)
                            );
                        }
                        else if (property.PropertyType == typeof(DateTime))
                        {
                            props.Date(d => d
                                .Name(propertyName)
                            );
                        }
                        else if (property.PropertyType == typeof(Guid))
                        {
                            props.Keyword(k => k
                                .Name(propertyName)
                            );
                        }
                        else if (property.PropertyType == typeof(bool))
                        {
                            props.Boolean(b => b
                                .Name(propertyName)
                            );
                        }
                    }

                    props.Date(d => d
                        .Name(CTime)
                    );
                    return props;
                })
            )
        );
        if (!createIndexResponse.IsValid)
        {
            _logger.LogError("Error creating gevent index {indexName} {error} {createIndexResponse}", indexName, createIndexResponse.ServerError?.Error, JsonConvert.SerializeObject(createIndexResponse));
        }
        else
        {
            _logger.LogInformation("Index created gevent successfully. {indexName}", indexName);
        }
    }

    public async Task SaveOrUpdateGEventIndexAsync<T>(T gEvent) where T : GEventBase
    {
        var indexName = gEvent.GetType().Name.ToLower() + IndexSuffix;
        var properties = gEvent.GetType().GetProperties();
        var document = new Dictionary<string, object>();

        foreach (var property in properties)
        {
            var value = property.GetValue(gEvent);
            document.Add(property.Name, value);
        }
        document.Add(CTime, DateTime.Now);

        var response = await _elasticClient.IndexAsync(document , i => i
            .Index(indexName)
            .Id(gEvent.Id)
        );

        if (!response.IsValid)
        {
            _logger.LogError("{indexName} save Error, indexing document error:{error} ,response:{response}" ,indexName, response.ServerError, JsonConvert.SerializeObject(response));
        }
        else
        {
            _logger.LogInformation("{indexName} save Successfully.",indexName);
        }
    }
    
    public async Task<string> QueryEventIndexAsync(string id, string indexName)
    {
        try
        {
            var response = await _elasticClient.GetAsync<dynamic>(id, g => g.Index(indexName)); 
            var source = response.Source;
            if (source == null)
            {
                return "";
            }
            var documentContent = JsonConvert.SerializeObject(source);
            return documentContent;
        }
        catch (Exception e)
        {
            _logger.LogError("{indexName} ,id:{id}QueryEventIndexAsync fail.", indexName,id);
            throw e;
        }
        
    }

    public async Task<string> QueryEventIndexAsync(DateTime beginDateTime, DateTime endDateTime, string indexName)
    {
        try
        {
            var response = await _elasticClient.SearchAsync<dynamic>(s => s
                .Index(indexName) // Specify the index name
                .Query(q => q
                    .DateRange(r => r
                            .Field("ctime") // Specify the date field
                            .GreaterThanOrEquals(beginDateTime) // Start time >=
                            .LessThanOrEquals(endDateTime) // End time <=
                    )
                )
            );
            if (!response.IsValid)
            {
                _logger.LogError("Error QueryEventIndexAsync index. {indexName} ,{error}",indexName, response.ServerError?.Error);
            }

            var source = response.Documents;
            if (source == null)
            {
                return "";
            }
            var documentContent = JsonConvert.SerializeObject(source);
            return documentContent;
        }
        catch (Exception e)
        {
            _logger.LogError("{indexName} ,beginDateTime:{beginDateTime} ,endDateTime:{endDateTime}  QueryEventIndexAsync fail.", indexName,beginDateTime,endDateTime);
            throw;
        }
    }

    public void CheckExistOrCreateIndex<T>() where T : class
    {
        var indexName = typeof(T).Name.ToLower();
        var indexExistsResponse = _elasticClient.Indices.Exists(indexName);
        if (indexExistsResponse.Exists)
        {
            return;
        }
        var createIndexResponse = _elasticClient.Indices.Create(indexName, c => c
            .Map<T>(m => m.AutoMap())
        );
        if (!createIndexResponse.IsValid)
        {
            _logger.LogError("Error creating index. {indexName} ,{error} ,{createIndexResponse}",indexName, createIndexResponse.ServerError?.Error,JsonConvert.SerializeObject(createIndexResponse));
        }
        else
        {
            _logger.LogError("Index created successfully. {indexName}", indexName);
        }
    }

    public async Task SaveOrUpdateChatLogIndexAsync(AIChatLogIndex index)
    {
        var indexName = index.GetType().Name.ToLower();
        var response = await _elasticClient.IndexAsync(index, i => i
            .Index(indexName)
            .Id(index.Id)
        );
        
        if (!response.IsValid)
        {
            _logger.LogInformation("{indexName} save Error, indexing document error:{error} response:{response}: " ,indexName, response.ServerError,JsonConvert.SerializeObject(response));
        }
        else
        {
            _logger.LogInformation("{indexName} save Successfully.",indexName);
        }
    }

    public async Task<(long TotalCount,List<AIChatLogIndex> ChatLogs)> QueryChatLogListAsync(ChatLogQueryInputDto input)
    {
        if (input == null)
        {
            return (0, new List<AIChatLogIndex>());
        }

        if (input.BeginTimestamp > input.EndTimestamp)
        {
            return (0, new List<AIChatLogIndex>());
        }

        var mustQuery = new List<Func<QueryContainerDescriptor<AIChatLogIndex>, QueryContainer>>();
        if (!input.GroupId.IsNullOrEmpty())
        {
            mustQuery.Add(q => q.Term(i
                => i.Field(f => f.GroupId).Field(input.GroupId)));
        }
        
        if (!input.AgentId.IsNullOrEmpty())
        {
            mustQuery.Add(q => q.Term(i
                => i.Field(f => f.AgentId).Field(input.AgentId)));
        }
        
        if (input.Ids?.Count > 0)
        {
            mustQuery.Add(q => q.Terms(i => i.Field(f => f.Id).Terms(input.Ids)));
        }
        
        if (input.BeginTimestamp > 0)
        {
            mustQuery.Add(q => q.DateRange(i =>
                i.Field(f => f.Ctime)
                    .GreaterThanOrEquals(DateTime.UnixEpoch.AddMilliseconds((double)input.BeginTimestamp))));
        }

        if (input.EndTimestamp > 0)
        {
            mustQuery.Add(q => q.DateRange(i =>
                i.Field(f => f.Ctime)
                    .LessThanOrEquals(DateTime.UnixEpoch.AddMilliseconds((double)input.EndTimestamp))));
        }
        QueryContainer Filter(QueryContainerDescriptor<AIChatLogIndex> f)
            => f.Bool(b => b.Must(mustQuery));
        var searchResponse = _elasticClient.Search<AIChatLogIndex>(s => s
            .Index(nameof(AIChatLogIndex).ToLower())
            .Query(q => q
                .Bool(b => b
                    .Must(mustQuery)
                )
            )
            .From(input.SkipCount)
            .Size(input.MaxResultCount)
            .Sort(ss => ss
                    .Ascending(a => a.Ctime)
            )
        );
        switch (searchResponse.IsValid)
        {
            case true:
            {
                var chatLogIndexList = new List<AIChatLogIndex>();
                if (searchResponse.Total != 0)
                {
                    chatLogIndexList = searchResponse.Documents.ToList();
                }
                return (searchResponse.Total, chatLogIndexList);
            }
            default:
                _logger.LogInformation("QueryChatLogListAsync fail errMsg:{errMsg}.",searchResponse.ServerError);
                break;
        }
        return (0, new List<AIChatLogIndex>());
    }
}
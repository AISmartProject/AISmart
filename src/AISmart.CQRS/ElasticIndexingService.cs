using System.Threading.Tasks;
using AISmart.CQRS.Dto;
using Microsoft.Extensions.Logging;
using Nest;
using Newtonsoft.Json;

namespace AISmart.CQRS;

public class ElasticIndexingService : IIndexingService
{
    private readonly IElasticClient _elasticClient;
    private readonly ILogger<ElasticIndexingService> _logger;

    public ElasticIndexingService(ILogger<ElasticIndexingService> logger, IElasticClient elasticClient)
    {
        _logger = logger;
        _elasticClient = elasticClient;
    }

    public void CheckExistOrCreateIndex(string typeName)
    {
        var indexName = typeName.ToLower() + "index";
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
            _logger.LogError("Error creating index {error}", createIndexResponse.ServerError?.Error);
        }
        else
        {
            _logger.LogError("Index created successfully. {indexName}", indexName);
        }
    }

    public async Task SaveOrUpdateIndexAsync(string typeName, BaseStateIndex baseStateIndex)
    {
        _logger.LogInformation("save state info start....index:{index},id:{id}",typeName,baseStateIndex.Id);
        var indexName = typeName.ToLower() + "index";
        await _elasticClient.IndexAsync(baseStateIndex, i => i
            .Index(indexName)
            .Id(baseStateIndex.Id)
        );
        _logger.LogInformation("save state info end....index:{index},id:{id},state:{state}",typeName,baseStateIndex.Id,JsonConvert.SerializeObject(baseStateIndex));

    }

    public async Task<BaseStateIndex> QueryIndexAsync(string id,string indexName)
    {
        var response = await _elasticClient.GetAsync<BaseStateIndex>(id, g => g.Index(indexName));
        return response.Source; 
    }
}
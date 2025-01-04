using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using AISmart.Dto;
using AiSmart.GAgent.TestAgent.NamingContest.Common;
using AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;
using AISmart.Options;
using AISmart.PumpFun;
using AISmart.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace AISmart.Provider;

public class NameContestProvider : INameContestProvider,ISingletonDependency
{
    private readonly ILogger<NameContestProvider> _logger;



    public NameContestProvider(ILogger<NameContestProvider> logger)
    {
        _logger = logger;
    }
    
    public async Task SendMessageAsync(Guid groupId,NamingLogEvent? namingLogEvent,string callBackUrl)
    {
        
        // Serialize the request object to JSON
        var json = JsonConvert.SerializeObject(namingLogEvent, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        });
        
        

        try
        {
            JsonNode? jsonObject = JsonNode.Parse(json);
            jsonObject!["GroupId"] = groupId;
            var updatedJson = jsonObject.ToString();
            
            _logger.LogDebug("NameContestProvider send message  {replyMessage} to  {addresss}",updatedJson, callBackUrl);
            var client = new HttpClient();
            client.BaseAddress = new Uri(callBackUrl);
            
            var response = await client.PostAsync(callBackUrl, new StringContent(updatedJson, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            if (namingLogEvent != null)
            {
                _logger.LogDebug("NameContestProvider send message end  {replyId} : {response}", namingLogEvent.EventId,
                    response);
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation(responseBody);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError($"request NameContest callback url error: {e.Message}");
        }
    }
}
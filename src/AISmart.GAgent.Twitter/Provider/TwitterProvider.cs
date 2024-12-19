using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AElfScanServer.Worker.Core.Dtos;
using AISmart.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace AISmart.Provider;

public interface ITwitterProvider
{
    public  Task<List<Tweet>> GetLatestTwittersAsync(string sendUser, string userId, string sinceTweetId);

    public Task<string> PostTwitterAsync(string sendUser, string message);
}


public class TwitterProvider : ITwitterProvider, ISingletonDependency
{
    private readonly ILogger<ITwitterProvider> _logger;
    private readonly IOptionsMonitor<TwitterOptions> _telegramOptions;
    public TwitterProvider(ILogger<ITwitterProvider> logger,IOptionsMonitor<TwitterOptions> telegramOptions)
    {
        _logger = logger;
        _telegramOptions = telegramOptions;
    }

    public async Task<List<Tweet>> GetLatestTwittersAsync(string sendUser, string userId, string sinceTweetId)
    {
        var bearerToken = GetAccount(sendUser);
        string url = $"https://api.twitter.com/2/tweets"; 
        int maxResults = 3; 
        _logger.LogInformation($"twitterToken = {bearerToken}, userId = {userId},sinceTweetId = {sinceTweetId}");

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            var requestUrl = $"{url}?max_results={maxResults}&since_id={{sinceTweetId}}";
            var response = await client.GetAsync(requestUrl);
          
            if (response.Headers.Contains("x-rate-limit-limit"))
            {
                var rateLimit = response.Headers.GetValues("x-rate-limit-limit");
                var rateLimitRemaining = response.Headers.GetValues("x-rate-limit-remaining");
                var rateLimitReset = response.Headers.GetValues("x-rate-limit-reset");

                _logger.LogInformation($"rateLimit = {rateLimit.First()}, rateLimitRemaining = {rateLimitRemaining.First()},rateLimitReset = {rateLimitReset.First()}");
                if (rateLimitRemaining.First() == "0")
                {
                    var resetTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(rateLimitReset.First()));
                    var waitTime = resetTime - DateTimeOffset.UtcNow;
                    _logger.LogInformation($"waitTime {waitTime.TotalSeconds} ");
                }
            }

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("responseBody: " + responseBody);
                var responseData = JsonConvert.DeserializeObject<TwitterResponseDto>(responseBody);
                return responseData.Tweets;
            }
          
            string errorResponse = await response.Content.ReadAsStringAsync();
            _logger.LogWarning($"response failed，code: {response.StatusCode}, body: {errorResponse}");
            return null;
        }
    }
    
    private string GetAccount(string accountName)
    {
        var optionExists = _telegramOptions.CurrentValue.AccountDictionary.TryGetValue(accountName, out var account);
        if (!optionExists)
        {
            throw new UserFriendlyException($"Twitter Account {accountName} not found");
        }
        return account;
    }


    public async Task<string> PostTwitterAsync(string sendUser, string message)
    {
        var tweetData = new
        {
            text = message
        };
        var jsonContent = JsonConvert.SerializeObject(tweetData);
   
        using (var httpClient = new HttpClient())
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.twitter.com/2/tweets")
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            };
            var bearerToken = GetAccount(sendUser);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
   
            // 发送请求
            HttpResponseMessage response = await httpClient.SendAsync(request);
   
            // 处理响应
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Successfully posted tweet!");
                var responseData = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response: " + responseData);
            }
            else
            {
                Console.WriteLine($"Failed to post tweet: {response.StatusCode}");
                var errorData = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Error: " + errorData);
            }
        }
        return "";
    }

   
       
}
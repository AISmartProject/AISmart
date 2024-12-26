using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AElfScanServer.Worker.Core.Dtos;
using AISmart.Options;
using AISmart.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace AISmart.Provider;

public interface ITwitterProvider
{
    public  Task<List<Tweet>> GetLatestTwittersAsync(string sendUser, string userId, string sinceTweetId);
    public Task<string> PostTwitterAsync(string message, string accessToken, string accessTokenSecret);
    public Task<string> ReplyAsync(string message, string tweetId, string accessToken, string accessTokenSecret);
    // public Task<List<Tweet>> GetUserTweetsAsync();
}


public class TwitterProvider : ITwitterProvider, ISingletonDependency
{
    private readonly ILogger<ITwitterProvider> _logger;
    private readonly IOptionsMonitor<TwitterOptions> _twitterOptions;
    private readonly HttpClient _httpClient;
    private readonly AESCipher _aesCipher;
    
    public TwitterProvider(ILogger<ITwitterProvider> logger, IOptionsMonitor<TwitterOptions> twitterOptions)
    {
        _logger = logger;
        _twitterOptions = twitterOptions;
        _httpClient = new HttpClient();
        string password = twitterOptions.CurrentValue.EncryptionPassword;
        _aesCipher = new AESCipher(password);
    }

    public async Task<List<Tweet>> GetLatestTwittersAsync(string sendUser, string userId, string sinceTweetId)
    {
        var bearerToken = GetAccountInfo(sendUser).BearerToken;
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
    
    /// <summary>
    /// 获取最新推文
    /// </summary>
    /// <returns>推文列表</returns>
    // public async Task<List<Tweet>> GetUserTweetsAsync()
    // {
    //     var url = "https://api.twitter.com/2/users/me/tweets?max_results=100";
    //
    //     // 生成OAuth 1.0a Authorization头
    //     string authHeader = GenerateOAuthHeader("GET", url);
    //
    //     var request = new HttpRequestMessage(HttpMethod.Get, url);
    //     request.Headers.TryAddWithoutValidation("Authorization", authHeader);
    //     request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    //
    //     HttpResponseMessage response = await _httpClient.SendAsync(request);
    //
    //     if (response.IsSuccessStatusCode)
    //     {
    //         var responseData = await response.Content.ReadAsStringAsync();
    //         dynamic json = JsonConvert.DeserializeObject(responseData);
    //         List<Tweet> tweets = new List<Tweet>();
    //         foreach (var tweet in json.data)
    //         {
    //             tweets.Add(new Tweet
    //             {
    //                 Id = tweet.id,
    //                 Text = tweet.text
    //             });
    //         }
    //         return tweets;
    //     }
    //     else
    //     {
    //         Console.WriteLine($"获取推文失败: {response.StatusCode}");
    //         var errorData = await response.Content.ReadAsStringAsync();
    //         Console.WriteLine("错误: " + errorData);
    //         return new List<Tweet>();
    //     }
    // }
    
    private AccountInfo GetAccountInfo(string accountName)
    {
        var optionExists = _twitterOptions.CurrentValue.AccountDictionary.TryGetValue(accountName, out var account);
        if (!optionExists)
        {
            throw new UserFriendlyException($"Twitter Account {accountName} not found");
        }
        return account;
    }
    
    private string GetDecryptedData(string data)
    {
        try
        {
            return  _aesCipher.Decrypt(data);
        }
        catch (Exception e)
        {
            _logger.LogError(e,$"Decrypt error: {data}");
        }
        return "";
    }
    
    public async Task<string> PostTwitterAsync(string message, string accessToken, string accessTokenSecret)
    {
        var url = "https://api.twitter.com/2/tweets";

        // accessToken = GetDecryptedData(accessToken);
        // accessTokenSecret = GetDecryptedData(accessTokenSecret);
        string authHeader = GenerateOAuthHeader("POST", url, accessToken, accessTokenSecret);

        var jsonBody = JsonConvert.SerializeObject(new { text = message });

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
        };

        requestMessage.Headers.TryAddWithoutValidation("Authorization", authHeader);
        requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        
        HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);
        if (response.IsSuccessStatusCode)
        {
            var responseData = await response.Content.ReadAsStringAsync();
            return responseData;
        }
        else
        {
            var errorData = await response.Content.ReadAsStringAsync();
            return $"Error: {errorData}";
        }
    }
    
    public async Task<string> ReplyAsync(string message, string tweetId, string accessToken, string accessTokenSecret)
    {
        var url = "https://api.twitter.com/2/tweets";
        
        string authHeader = GenerateOAuthHeader("POST", url, accessToken, accessTokenSecret);
        
        var jsonBody = JsonConvert.SerializeObject(new
        {
            text = message,
            reply = new
            {
                in_reply_to_tweet_id = tweetId
            }
        });
        
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
        };
        
        requestMessage.Headers.TryAddWithoutValidation("Authorization", authHeader);
        requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        
        HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);
        if (response.IsSuccessStatusCode)
        {
            var responseData = await response.Content.ReadAsStringAsync();
            return responseData;
        }
        else
        {
            var errorData = await response.Content.ReadAsStringAsync();
            return $"Error: {errorData}";
        }
        
    }
    
    private string GenerateOAuthHeader(string httpMethod, string url, string accessToken, string accessTokenSecret)
    {
        var consumerKey = _twitterOptions.CurrentValue.ConsumerKey;
        var consumerSecret = _twitterOptions.CurrentValue.ConsumerSecret;
      
        var oauthParameters = new Dictionary<string, string>
        {
            { "oauth_consumer_key", consumerKey },
            { "oauth_nonce", Guid.NewGuid().ToString("N") },
            { "oauth_signature_method", "HMAC-SHA1" },
            { "oauth_timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() },
            { "oauth_token", accessToken },
            { "oauth_version", "1.0" }
        };
        
        var allParams = new Dictionary<string, string>(oauthParameters);
        
        var sortedParams = allParams.OrderBy(kvp => kvp.Key).ThenBy(kvp => kvp.Value);
        var parameterString = string.Join("&", sortedParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

        var signatureBaseString = $"{httpMethod.ToUpper()}&{Uri.EscapeDataString(url)}&{Uri.EscapeDataString(parameterString)}";
        
        var signingKey = $"{Uri.EscapeDataString(consumerSecret)}&{Uri.EscapeDataString(accessTokenSecret)}";
        
        string oauthSignature;
        using (var hasher = new HMACSHA1(Encoding.ASCII.GetBytes(signingKey)))
        {
            var hash = hasher.ComputeHash(Encoding.ASCII.GetBytes(signatureBaseString));
            oauthSignature = Convert.ToBase64String(hash);
        }
        
        allParams.Add("oauth_signature", oauthSignature);

        var authHeader = "OAuth " + string.Join(", ",
            allParams.OrderBy(kvp => kvp.Key)
                    .Where(kvp => kvp.Key.StartsWith("oauth_"))
                    .Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}=\"{Uri.EscapeDataString(kvp.Value)}\""));

        return authHeader;
    }
    
}


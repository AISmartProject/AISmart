using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AISmart.Dto;
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
    public Task<string> PostTwitterAsync(string message, string accessToken, string accessTokenSecret);
    public Task<string> ReplyAsync(string message, string tweetId, string accessToken, string accessTokenSecret);
    public Task<List<Tweet>> GetMentionsAsync();
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
    
    public async Task<List<Tweet>> GetMentionsAsync()
    {
        var bearerToken = _twitterOptions.CurrentValue.BearerToken;
        string username = "elonmusk";
        string query = $"@{username}";
        string encodedQuery = Uri.EscapeDataString(query);
        string url = $"https://api.twitter.com/2/tweets/search/recent?query={encodedQuery}&tweet.fields=author_id,conversation_id&max_results=100";

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("responseBody: " + responseBody);
                var responseData = JsonConvert.DeserializeObject<TwitterResponseDto>(responseBody);
                return responseData.Data;
            }
          
            string errorResponse = await response.Content.ReadAsStringAsync();
            _logger.LogWarning($"response failed，code: {response.StatusCode}, body: {errorResponse}");
            return new List<Tweet>();
        }
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

        accessToken = GetDecryptedData(accessToken);
        accessTokenSecret = GetDecryptedData(accessTokenSecret);
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
        
        accessToken = GetDecryptedData(accessToken);
        accessTokenSecret = GetDecryptedData(accessTokenSecret);
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
    
    private string GenerateOAuthHeader(string httpMethod, string url, string accessToken, string accessTokenSecret, Dictionary<string, string> additionalParams = null)
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
        if (additionalParams != null)
        {
            foreach (var param in additionalParams)
            {
                allParams.Add(param.Key, param.Value);
            }
        }
        
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


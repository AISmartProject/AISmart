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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace AISmart.Provider;

public interface ITwitterProvider
{
    public  Task<List<Tweet>> GetLatestTwittersAsync(string sendUser, string userId, string sinceTweetId);
    public Task<string> PostTwitterAsync(string message, string accountName);
    public Task<string> ReplyAsync(string message, string tweetId);
}


public class TwitterProvider : ITwitterProvider, ISingletonDependency
{
    private readonly ILogger<ITwitterProvider> _logger;
    private readonly IOptionsMonitor<TwitterOptions> _twitterOptions;
    
    private const string consumerKey = "IEZnojYHqJ9Ic8LEfUoBAAQGB";
    private const string consumerSecret = "aTby5FHleJvNred6IGvPshQ2mkzRepU2k3dcu1Ry5TclsraBuP";
    private const string accessToken = "1871763829518139392-wZTabRYQ4qNZ4LaMwkaEXPYXOVzjbR";
    private const string accessTokenSecret = "J4uuPoPNgKxp9bLpVNR4Qfvmp4SdudDsGveNQyMG5W5bL";
    
    public TwitterProvider(ILogger<ITwitterProvider> logger, IOptionsMonitor<TwitterOptions> twitterOptions)
    {
        _logger = logger;
        _twitterOptions = twitterOptions;
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
    
    private AccountInfo GetAccountInfo(string accountName)
    {
        var optionExists = _twitterOptions.CurrentValue.AccountDictionary.TryGetValue(accountName, out var account);
        if (!optionExists)
        {
            throw new UserFriendlyException($"Twitter Account {accountName} not found");
        }
        return account;
    }
    
    public async Task<string> PostTwitterAsync(string message, string accountName)
    {
        var accountInfo = GetAccountInfo(accountName);
        
        var consumerKey = accountInfo.ConsumerKey;
        var consumerSecret = accountInfo.ConsumerSecret;
        var accessToken = accountInfo.AccessToken;
        var accessTokenSecret = accountInfo.AccessTokenSecret;
        
        // 使用 API v2 发布推文
        var url = "https://api.twitter.com/2/tweets";

        // OAuth 参数
        var oauthParameters = new Dictionary<string, string>
        {
            { "oauth_consumer_key", consumerKey },
            { "oauth_nonce", Guid.NewGuid().ToString("N") },
            { "oauth_signature_method", "HMAC-SHA1" },
            { "oauth_timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() },
            { "oauth_token", accessToken },
            { "oauth_version", "1.0" }
        };

        // 请求参数
        var requestParameters = new Dictionary<string, string>
        {
            { "text", message }
        };

        // 合并所有参数用于签名
        var allParameters = new Dictionary<string, string>(oauthParameters);

        // 构建签名字符串
        var sortedParams = allParameters.OrderBy(kvp => kvp.Key).ThenBy(kvp => kvp.Value);
        var parameterString = string.Join("&", sortedParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        var signatureBaseString = $"POST&{Uri.EscapeDataString(url)}&{Uri.EscapeDataString(parameterString)}";

        // 构建签名密钥
        var signingKey = $"{Uri.EscapeDataString(consumerSecret)}&{Uri.EscapeDataString(accessTokenSecret)}";

        // 生成签名
        string oauthSignature;
        using (var hasher = new HMACSHA1(Encoding.ASCII.GetBytes(signingKey)))
        {
            var hash = hasher.ComputeHash(Encoding.ASCII.GetBytes(signatureBaseString));
            oauthSignature = Convert.ToBase64String(hash);
        }

        // 添加签名到 OAuth 参数
        oauthParameters.Add("oauth_signature", oauthSignature);

        // 构建 Authorization 头
        var authHeader = "OAuth " + string.Join(", ",
            oauthParameters.OrderBy(kvp => kvp.Key)
                           .Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}=\"{Uri.EscapeDataString(kvp.Value)}\""));

        using (var httpClient = new HttpClient())
        {
            // 构建 JSON 请求体
            var jsonBody = JsonConvert.SerializeObject(new { text = message });

            // 创建 HTTP 请求
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };

            // 设置 Authorization 头
            requestMessage.Headers.TryAddWithoutValidation("Authorization", authHeader);

            // 设置 Accept 头
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // 发送请求
            HttpResponseMessage response = await httpClient.SendAsync(requestMessage);

            // 处理响应
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("成功发布推文！");
                var responseData = await response.Content.ReadAsStringAsync();
                Console.WriteLine("响应: " + responseData);
                return responseData;
            }
            else
            {
                Console.WriteLine($"发布推文失败: {response.StatusCode}");
                var errorData = await response.Content.ReadAsStringAsync();
                Console.WriteLine("错误: " + errorData);
                return $"Error: {errorData}";
            }
        }
    }
    
    public async Task<string> ReplyAsync(string message, string tweetId)
    {
        // 使用 API v2 发布推文
        var url = "https://api.twitter.com/2/tweets";

        // OAuth 参数
        var oauthParameters = new Dictionary<string, string>
        {
            { "oauth_consumer_key", consumerKey },
            { "oauth_nonce", Guid.NewGuid().ToString("N") },
            { "oauth_signature_method", "HMAC-SHA1" },
            { "oauth_timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() },
            { "oauth_token", accessToken },
            { "oauth_version", "1.0" }
        };

        // 合并所有参数用于签名
        var allParameters = new Dictionary<string, string>(oauthParameters);

        // 构建签名字符串
        var sortedParams = allParameters.OrderBy(kvp => kvp.Key).ThenBy(kvp => kvp.Value);
        var parameterString = string.Join("&", sortedParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        var signatureBaseString = $"POST&{Uri.EscapeDataString(url)}&{Uri.EscapeDataString(parameterString)}";

        // 构建签名密钥
        var signingKey = $"{Uri.EscapeDataString(consumerSecret)}&{Uri.EscapeDataString(accessTokenSecret)}";

        // 生成签名
        string oauthSignature;
        using (var hasher = new HMACSHA1(Encoding.ASCII.GetBytes(signingKey)))
        {
            var hash = hasher.ComputeHash(Encoding.ASCII.GetBytes(signatureBaseString));
            oauthSignature = Convert.ToBase64String(hash);
        }

        // 添加签名到 OAuth 参数
        oauthParameters.Add("oauth_signature", oauthSignature);

        // 构建 Authorization 头
        var authHeader = "OAuth " + string.Join(", ",
            oauthParameters.OrderBy(kvp => kvp.Key)
                           .Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}=\"{Uri.EscapeDataString(kvp.Value)}\""));

        using (var httpClient = new HttpClient())
        {
            var jsonBody = JsonConvert.SerializeObject(new
            {
                text = message,
                reply = new
                {
                    in_reply_to_tweet_id = tweetId
                }
            });

            // 创建 HTTP 请求
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };

            // 设置 Authorization 头
            requestMessage.Headers.TryAddWithoutValidation("Authorization", authHeader);

            // 设置 Accept 头
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // 发送请求
            HttpResponseMessage response = await httpClient.SendAsync(requestMessage);

            // 处理响应
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("成功发布推文！");
                var responseData = await response.Content.ReadAsStringAsync();
                Console.WriteLine("响应: " + responseData);
                return responseData;
            }
            else
            {
                Console.WriteLine($"发布推文失败: {response.StatusCode}");
                var errorData = await response.Content.ReadAsStringAsync();
                Console.WriteLine("错误: " + errorData);
                return $"Error: {errorData}";
            }
        }
    }
    
    private string GenerateOAuthHeader(string httpMethod, string url, Dictionary<string, string> additionalParams)
    {
        // OAuth参数
        var oauthParameters = new Dictionary<string, string>
        {
            { "oauth_consumer_key", consumerKey },
            { "oauth_nonce", Guid.NewGuid().ToString("N") },
            { "oauth_signature_method", "HMAC-SHA1" },
            { "oauth_timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() },
            { "oauth_token", accessToken },
            { "oauth_version", "1.0" }
        };

        // 合并所有参数用于签名
        var allParams = new Dictionary<string, string>(oauthParameters);
        if (additionalParams != null)
        {
            foreach (var param in additionalParams)
            {
                allParams.Add(param.Key, param.Value);
            }
        }

        // 构建签名字符串
        var sortedParams = allParams.OrderBy(kvp => kvp.Key).ThenBy(kvp => kvp.Value);
        var parameterString = string.Join("&", sortedParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

        var signatureBaseString = $"{httpMethod.ToUpper()}&{Uri.EscapeDataString(url)}&{Uri.EscapeDataString(parameterString)}";

        // 构建签名密钥
        var signingKey = $"{Uri.EscapeDataString(consumerSecret)}&{Uri.EscapeDataString(accessTokenSecret)}";

        // 生成签名
        string oauthSignature;
        using (var hasher = new HMACSHA1(Encoding.ASCII.GetBytes(signingKey)))
        {
            var hash = hasher.ComputeHash(Encoding.ASCII.GetBytes(signatureBaseString));
            oauthSignature = Convert.ToBase64String(hash);
        }

        // 添加签名到OAuth参数
        allParams.Add("oauth_signature", oauthSignature);

        // 构建Authorization头
        var authHeader = "OAuth " + string.Join(", ",
            allParams.OrderBy(kvp => kvp.Key)
                    .Where(kvp => kvp.Key.StartsWith("oauth_"))
                    .Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}=\"{Uri.EscapeDataString(kvp.Value)}\""));

        return authHeader;
    }
    
}


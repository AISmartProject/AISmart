using System.Collections.Generic;

namespace AISmart.Options;

public class TwitterOptions
{
    public Dictionary<string, AccountInfo> AccountDictionary { get; set; } = new();
}

public class AccountInfo
{
    public string BearerToken { get; set; }
    public string ConsumerKey { get; set; }
    public string ConsumerSecret { get; set; }
    public string AccessToken { get; set; }
    public string AccessTokenSecret { get; set; }
}
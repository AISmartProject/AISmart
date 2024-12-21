using System.Collections.Immutable;
using AElf;
using AElf.Types;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Identity;
using Volo.Abp.OpenIddict;
using Volo.Abp.OpenIddict.ExtensionGrantTypes;

namespace AISmart.AuthServer;

public class SignatureGrantHandler: ITokenExtensionGrant
{
    private ILogger<SignatureGrantHandler>? _logger;

    public string Name { get; } = "signature";

    public async Task<IActionResult> HandleAsync(ExtensionGrantContext context)
    {
       
        var timestampVal = context.Request.GetParameter("timestamp").ToString();
        var userName = context.Request.GetParameter("user_name").ToString();
        var userId = context.Request.GetParameter("user_id").ToString();
        

        var invalidParamResult = CheckParams(timestampVal,userName);
        if (invalidParamResult != null)
        {
            return invalidParamResult;
        }
        var timestamp = long.Parse(timestampVal!);

        var time = DateTime.UnixEpoch.AddMilliseconds(timestamp);
        var timeRangeConfig = context.HttpContext.RequestServices.GetRequiredService<IOptionsSnapshot<TimeRangeOption>>()
            .Value;

        if (time < DateTime.UtcNow.AddMinutes(-timeRangeConfig.TimeRange) ||
            time > DateTime.UtcNow.AddMinutes(timeRangeConfig.TimeRange))
        {
            return GetForbidResult(OpenIddictConstants.Errors.InvalidRequest,
                $"The time should be {timeRangeConfig.TimeRange} minutes before and after the current time.");
        }
        _logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<SignatureGrantHandler>>();
        var userManager = context.HttpContext.RequestServices.GetRequiredService<IdentityUserManager>();
        var user = await userManager.FindByNameAsync(userName!);
        _logger.LogInformation("AISmartAuthServer user login:loginUser:{userName}, userDto:{user}", 
            userName,JsonConvert.SerializeObject(user));

        if (user == null)
        {
            _logger.LogInformation("AISmartAuthServer user login:loginUser:{user} is not exist", userName);

            var userIdGuid = Guid.NewGuid();

            var createUserResult = await CreateUserAsync(userManager, userIdGuid,userName:userName);
            if (!createUserResult)
            {
                return GetForbidResult(OpenIddictConstants.Errors.ServerError, "Create user failed.");
            }

            user = await userManager.GetByIdAsync(userIdGuid);
        }
        else
        {
            _logger.LogInformation("AISmartAuthServer user login:loginUser:{user} is exist,userDto:{user}", userName,user);
            if (userId != user.Id.ToString())
            {
                return GetForbidResult(OpenIddictConstants.Errors.InvalidRequest,
                    $"The user_id and user_name must match.");
            }

        }
        var signInManager = context.HttpContext.RequestServices.GetRequiredService<Microsoft.AspNetCore.Identity.SignInManager<IdentityUser>>();
        var principal = await signInManager.CreateUserPrincipalAsync(user);
        principal.SetScopes("AISmartAuthServer");
        principal.SetResources(await GetResourcesAsync(context, principal.GetScopes()));
        principal.SetAudiences("AISmartAuthServer");
        
        var abpOpenIddictClaimDestinationsManager = context.HttpContext.RequestServices
            .GetRequiredService<AbpOpenIddictClaimsPrincipalManager>();

        await abpOpenIddictClaimDestinationsManager.HandleAsync(context.Request, principal);
        return new SignInResult(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, principal);
    }
    
    private ForbidResult GetForbidResult(string errorType, string errorDescription)
    {
        return new ForbidResult(
            new[] { OpenIddictServerAspNetCoreDefaults.AuthenticationScheme },
            properties: new AuthenticationProperties(new Dictionary<string, string>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = errorType,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = errorDescription
            }!));
    }
    
    private static ForbidResult? CheckParams(string? timestampVal ,string? userName)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(userName))
        {
            errors.Add("invalid parameter source.");
        }

        if (string.IsNullOrWhiteSpace(timestampVal) || !long.TryParse(timestampVal, out var time) || time <= 0)
        {
            errors.Add("invalid parameter timestamp.");
        }

        if (errors.Count > 0)
        {
            return new ForbidResult(
                new[] { OpenIddictServerAspNetCoreDefaults.AuthenticationScheme },
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidRequest,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = GetErrorMessage(errors)
                }!));
        }

        return null;
    }
    
    private static string GetErrorMessage(List<string> errors)
    {
        var message = string.Empty;

        errors?.ForEach(t => message += $"{t}, ");
        if (message.Contains(','))
        {
            return message.TrimEnd().TrimEnd(',');
        }

        return message;
    }

    private async Task<bool> CreateUserAsync(IdentityUserManager userManager, Guid userId, 
        string userName)
    {
        var result = false;
        var user = new IdentityUser(userId, userName: userName, email: userName+ "@ai-smart.io");
        var identityResult = await userManager.CreateAsync(user);

        if (identityResult.Succeeded)
        {
            _logger.LogDebug($"create user success: {userId.ToString()}");
        }

        result = identityResult.Succeeded;

        return result;
    }

    private static async Task<IEnumerable<string>> GetResourcesAsync(ExtensionGrantContext context,
        ImmutableArray<string> scopes)
    {
        var resources = new List<string>();
        if (!scopes.Any())
        {
            return resources;
        }

        await foreach (var resource in context.HttpContext.RequestServices.GetRequiredService<IOpenIddictScopeManager>()
                           .ListResourcesAsync(scopes))
        {
            resources.Add(resource);
        }

        return resources;
    }
}
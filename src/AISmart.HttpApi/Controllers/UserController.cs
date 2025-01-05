using System.Threading.Tasks;
using AISmart.Service;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace AISmart.Controllers;

[RemoteService]
[ControllerName("Users")]
[Route("api/users")]
public class UserController :  AISmartController

{
    private readonly IUserAppService _userAppService;
    public UserController(IUserAppService userAppService)
    {
        _userAppService = userAppService;
    }
    
    [HttpPost("registerClient")]
    [Authorize(Policy = "OnlyAdminAccess")]
    public virtual async Task RegisterClientAuthentication(string clientId,string clientSecret)
    {
        await _userAppService.RegisterClientAuthentication(clientId, clientSecret);
    }
    
}
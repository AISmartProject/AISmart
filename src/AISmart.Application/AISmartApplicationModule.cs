﻿using AISmart.Application.Grains;
using AISmart.Options;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Account;
using Volo.Abp.AspNetCore.Mvc.Dapr;
using Volo.Abp.AutoMapper;
using Volo.Abp.Dapr;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;

namespace AISmart;

[DependsOn(
    typeof(AISmartDomainModule),
    typeof(AbpAccountApplicationModule),
    typeof(AISmartApplicationContractsModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpDaprModule),
    typeof(AbpAspNetCoreMvcDaprModule),
    typeof(AIApplicationGrainsModule),
    typeof(AISmartSimpleRagModule),
    typeof(AIApplicationGrainsModule),
    typeof(AISmartGAgentAElfModule),
    typeof(AISmartGAgentTelegramModule),
    typeof(AISmartGAgentTwitterModule)
)]
public class AISmartApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<AISmartApplicationModule>();
        });
        var configuration = context.Services.GetConfiguration();
        Configure<RagOptions>(configuration.GetSection("Rag"));
    }
}

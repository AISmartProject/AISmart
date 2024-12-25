﻿using AISmart;
using AISmart.Options;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace AiSmart.GAgent.SocialAgent;

[DependsOn(
    typeof(AISmartApplicationContractsModule),
    typeof(AISmartGAgentMicroAIModule)
)]
public class AISmartGAgentSocialGAgentModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<AISmartGAgentSocialGAgentModule>();
        });
        var configuration = context.Services.GetConfiguration();
        context.Services.Configure<MicroAIOptions>(configuration.GetSection("AutogenConfig"));
    }
}
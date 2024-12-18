using AISmart.Options;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace AISmart;

[DependsOn(
    typeof(AISmartApplicationContractsModule)
)]
public class AISmartRagModule : AbpModule
{
}
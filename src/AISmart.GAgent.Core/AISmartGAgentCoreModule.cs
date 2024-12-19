using Volo.Abp.Modularity;

namespace AISmart.GAgent.Core;

[DependsOn(
    typeof(AISmartApplicationContractsModule)
)]
public class AISmartGAgentCoreModule : AbpModule
{
    
}
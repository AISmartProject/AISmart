using AISmart.Core;
using Volo.Abp.Modularity;

namespace AISmart.GAgent.Core;

[DependsOn(
    typeof(AISmartCoreModule)
)]
public class AISmartGAgentCoreModule : AbpModule
{
    
}
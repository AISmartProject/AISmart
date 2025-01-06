using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace AISmart.GAgent.AtomicAgent;

[DependsOn(
    typeof(AbpAutoMapperModule),
    typeof(AISmartApplicationContractsModule)
)]
public class AISmartGAgentAtomicAgentModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<AISmartGAgentAtomicAgentModule>(); });
    }
}
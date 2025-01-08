using AISmart.Rag.Contracts;
using Volo.Abp.Modularity;

namespace AISmart;

[DependsOn(
    typeof(AISmartRagContractsModule)
)]
public class AISmartSimpleRagModule : AbpModule
{
}
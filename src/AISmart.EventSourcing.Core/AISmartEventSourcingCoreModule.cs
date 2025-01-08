using AISmart.Core;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace AISmart.EventSourcing.Core;

// ReSharper disable InconsistentNaming
[DependsOn(typeof(AISmartCoreModule))]
public class AISmartEventSourcingCoreModule : AbpModule
{
    
}
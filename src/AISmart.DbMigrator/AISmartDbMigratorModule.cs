using AISmart.MongoDB;
using AISmart.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Autofac;
using Volo.Abp.Caching;
using Volo.Abp.Identity;
using Volo.Abp.Identity.MongoDB;
using Volo.Abp.Modularity;
using IdentityRole = Volo.Abp.Identity.IdentityRole;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace AISmart.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AISmartMongoDbModule),
    typeof(AbpIdentityDomainModule),
    typeof(AbpIdentityMongoDbModule),
    typeof(AISmartApplicationContractsModule)
    )]
public class AISmartDbMigratorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<UsersOptions>(context.Services.GetConfiguration().GetSection("User"));
        Configure<AbpDistributedCacheOptions>(options => { options.KeyPrefix = "AISmart:"; });
        IdentityBuilderExtensions.AddDefaultTokenProviders(context.Services.AddIdentity<IdentityUser, IdentityRole>());
        context.Services.AddIdentity<IdentityUser, IdentityRole>()
            .AddDefaultTokenProviders();
    }
}

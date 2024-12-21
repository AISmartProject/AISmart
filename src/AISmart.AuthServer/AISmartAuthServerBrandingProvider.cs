using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace AISmart.AuthServer;

[Dependency(ReplaceServices = true)]
public class AISmartAuthServerBrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "AISmartAuthServer";
}

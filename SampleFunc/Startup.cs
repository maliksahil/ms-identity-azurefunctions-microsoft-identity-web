using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;

[assembly: FunctionsStartup(typeof(SampleFunc.Startup))]


namespace SampleFunc
{
    public class Startup : FunctionsStartup
    {
        public Startup()
        {
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            // This is configuration from environment variables, settings.json etc.
            var configuration = builder.GetContext().Configuration;

            builder.Services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = Constants.Bearer;
                sharedOptions.DefaultChallengeScheme = Constants.Bearer;
            })
                .AddMicrosoftIdentityWebApi(configuration)
                    .EnableTokenAcquisitionToCallDownstreamApi()
                    .AddDownstreamWebApi("DownstreamAPI", options => {
                        options.BaseUrl = configuration.GetValue<string>("DownstreamAPI:BaseUrl");
                        options.Scopes = configuration.GetValue<string>("DownstreamAPI:Scopes");
                    })
                    .AddInMemoryTokenCaches();
        }
    }
}

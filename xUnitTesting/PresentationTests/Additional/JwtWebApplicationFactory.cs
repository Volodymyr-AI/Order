using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace xUnitTesting.PresentationTests.Additional;

public sealed class JwtWebApplicationFactory : CustomWebApplicationFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("JwtTesting");
        builder.ConfigureAppConfiguration((ctx, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "nvgRXq3Fh1I2JxKDgtSGv5rPyws5puNuIkWe9cJGyT0="
            });
        });
        
        base.ConfigureWebHost(builder);
    }
}
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Mjolnir.Startup))]

namespace Mjolnir
{
	public class Startup : FunctionsStartup
	{
		public override void Configure(IFunctionsHostBuilder builder)
		{
			builder.Services.AddHttpClient();
		}
	}
}




using System;
using MyEdenSolution.Common.Blobs;
using MyEdenSolution.Common.Events;
using MyEdenSolution.Common.UserAuthentication;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(MyEdenSolution.MyEdenService.Service.Startup))]

namespace MyEdenSolution.MyEdenService.Service
{
    /// <summary>
    /// Initialized at that startup of the function.
    /// </summary>
    public class Startup : FunctionsStartup
    {
        /// <summary>
        /// Configures services for the function.
        /// </summary>
        /// <param name="builder">The <see cref="IFunctionsHostBuilder"/> instance that providers access to building in services.</param>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public override void Configure(IFunctionsHostBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.AddHttpClient();

            builder.Services.AddSingleton<IBlobRepository, BlobRepository>();
            builder.Services.AddSingleton<IEventGridPublisherService, EventGridPublisherService>();
            builder.Services.AddSingleton<IEventGridSubscriberService, EventGridSubscriberService>();
            builder.Services.AddSingleton<IUserAuthenticationService, QueryStringUserAuthenticationService>();
            builder.Services.AddSingleton<IMyEdenServiceTranscriptionService, MyEdenServiceTranscriptionService>();
        }
    }
}
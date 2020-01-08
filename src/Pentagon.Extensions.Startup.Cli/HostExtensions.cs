namespace Pentagon.Extensions.Startup.Cli {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public static class HostExtensions
    {
        public static async Task<int> RunCliAsync([NotNull] this IHost host, bool pressAnyBlock = false, CancellationToken cancellationToken =default)
        {
            var app = host.Services.GetService<ICliHostedService>();

            await host.RunAsync(cancellationToken).ConfigureAwait(false);

            if (pressAnyBlock)
            {
                Console.WriteLine();
                Console.WriteLine(value: " Press any key to exit the application...");
                Console.ReadKey();
            }

            if (app == null)
                return 0;

            return app.ResultCode.GetValueOrDefault();
        }
    }
}
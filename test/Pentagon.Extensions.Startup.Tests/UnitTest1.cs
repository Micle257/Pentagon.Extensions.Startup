

namespace Pentagon.Extensions.Startup.Tests
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    public class Tran
    {

    }

    public class Sigl
    {

    }

    public class Scop
    {

    }

    public class App : AppCore
    {
        /// <inheritdoc />
        protected override void BuildApp(IApplicationBuilder appBuilder, string[] args)
        {
            appBuilder.Services.AddTransient(c => new Tran())
                      .AddScoped(c => new Scop())
                      .AddSingleton(c => new Sigl());
        }

        /// <inheritdoc />
        protected override Task ExecuteCoreAsync(AppExecutionContext context)
        {
            Console.WriteLine(context.IterationCount);

            var t = context.Provider.GetRequiredService<Tran>();
            var si = context.Provider.GetRequiredService<Sigl>();
            var sc = context.Provider.GetRequiredService<Scop>();

            if (context.IterationCount == 50)
                context.TerminationRequested = true;

            return Task.CompletedTask;
        }
    }

    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var app = new App();

            app.ConfigureServices();

            app.Execute(AppExecutionType.SingleRun, new ExecutionOptions {LoopWaitMilliseconds = 10});
        }
    }
}

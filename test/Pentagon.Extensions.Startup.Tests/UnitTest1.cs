

namespace Pentagon.Extensions.Startup.Tests
{
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public class App : AppCore
    {
        /// <inheritdoc />
        protected override void BuildApp(IApplicationBuilder appBuilder, string[] args)
        {
            
        }

        /// <inheritdoc />
        protected override Task ExecuteScopedCoreAsync(AppExecutionContext context)
        {
            Console.WriteLine(context.IterationCount);

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

            app.Execute(AppExecutionType.LoopRun, new ExecutionOptions {LoopWaitMilliseconds = 10});
        }
    }
}

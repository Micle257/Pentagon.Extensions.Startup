namespace Pentagon.Extensions.Startup.Cli {
    using System.Threading;

    public interface IProgramCancellationSource
    {
        CancellationTokenSource CancellationTokenSource { get; }

        void Cancel();
    }
}
namespace Pentagon.Extensions.Startup.Cli {
    using System.Threading;

    public interface IProgramCancellationSource
    {
        CancellationToken Token { get; }

        CancellationTokenSource TokenSource { get; }

        void Cancel();
    }
}
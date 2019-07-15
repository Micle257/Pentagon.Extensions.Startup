namespace Pentagon.Extensions.Startup.Cli {
    using System;
    using System.Threading;
    using JetBrains.Annotations;

    class ProgramCancellationSource : IProgramCancellationSource, IDisposable
    {
        public ProgramCancellationSource(CancellationTokenSource tokenSource)
        {
            TokenSource = tokenSource ?? new CancellationTokenSource();
        }

        /// <inheritdoc />
        public CancellationToken Token => TokenSource.Token;

        /// <inheritdoc />
        [NotNull]
        public CancellationTokenSource TokenSource { get; }

        /// <inheritdoc />
        public void Cancel()
        {
            TokenSource.Cancel();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            TokenSource.Dispose();
        }
    }
}
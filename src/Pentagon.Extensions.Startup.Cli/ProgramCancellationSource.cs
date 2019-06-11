namespace Pentagon.Extensions.Startup.Cli {
    using System;
    using System.Threading;
    using JetBrains.Annotations;

    class ProgramCancellationSource : IProgramCancellationSource
    {
        public ProgramCancellationSource([NotNull] CancellationTokenSource tokenSource)
        {
            CancellationTokenSource = tokenSource ?? throw new ArgumentNullException(nameof(tokenSource));
        }

        /// <inheritdoc />
        [NotNull]
        public CancellationTokenSource CancellationTokenSource { get; }

        /// <inheritdoc />
        public void Cancel()
        {
            CancellationTokenSource.Cancel();;
        }
    }
}
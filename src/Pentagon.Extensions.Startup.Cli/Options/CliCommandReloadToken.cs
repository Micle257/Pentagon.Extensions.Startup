namespace Pentagon.Extensions.Startup.Cli {
    using System;
    using System.Threading;
    using Microsoft.Extensions.Primitives;

    public class CliCommandReloadToken : IChangeToken
    {
        private CancellationTokenSource _cts = new CancellationTokenSource();

        /// <inheritdoc />
        public bool ActiveChangeCallbacks => true;

        /// <inheritdoc />
        public bool HasChanged => _cts.IsCancellationRequested;

        /// <inheritdoc />
        public IDisposable RegisterChangeCallback(Action<object> callback, object state) => _cts.Token.Register(callback, state);

        /// <summary>
        /// Used to trigger the change token when a reload occurs.
        /// </summary>
        public void OnReload() => _cts.Cancel();
    }
}
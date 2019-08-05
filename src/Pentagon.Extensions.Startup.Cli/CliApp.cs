// -----------------------------------------------------------------------
//  <copyright file="AppConsoleCore.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup.Cli
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Console;
    using JetBrains.Annotations;
    using Logging;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Threading;

    public class CliApp : AppCore
    {
        /// <inheritdoc />
        protected override void BuildApp(IApplicationBuilder appBuilder, string[] args)
        {
            appBuilder.AddCliCommands();
        }

        /// <inheritdoc />
        protected override void OnPostConfigureServices()
        {
            Debug.Assert(Environment != null, nameof(Environment) + " != null");
            Debug.Assert(Environment.ApplicationName != null, "Environment.ApplicationName != null");

            Console.Title = Environment.ApplicationName;
        }

        public virtual void OnExit(int statusCode)
        {
            Console.WriteLine();
            Console.WriteLine();

            if (!statusCode.IsAnyEqual(StatusCodes.Success, StatusCodes.Cancel))
            {
                ConsoleWriter.WriteError(errorValue: "Program execution failed.");
                Console.WriteLine();
            }

            if (statusCode == StatusCodes.Cancel)
            {
                ConsoleWriter.WriteError(errorValue: "Program canceled.");
                Console.WriteLine();
            }

#if !DEBUG
            if (Environment.IsDevelopment())
            {
                Console.WriteLine(value: " Press any key to exit the application...");
                Console.ReadKey();
            }
#endif
        }

        public void UpdateOptions<TOptions>([CanBeNull] Action<TOptions> updateCallback)
        {
            UpdateOptions(Options.DefaultName, updateCallback);
        }

        public void UpdateOptions<TOptions>(string name, [CanBeNull] Action<TOptions> updateCallback)
        {
            var options = Services.GetServices<ICliOptionsSource<TOptions>>()
                                  .FirstOrDefault(a => a.Name == name);

            if (options == null)
                return;

            updateCallback?.Invoke(options.Options);

            options.Reload();
        }

        public async Task<int> ExecuteCliAsync(string[] args, CancellationToken cancellationToken = default)
        {
            if (!cancellationToken.CanBeCanceled)
            {
                cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, Services.GetService<IProgramCancellationSource>().Token).Token;
            }

            try
            {
                if (_parallelCallbacks.Count == 0)
                    return await ExecuteCliCoreAsync(args).ConfigureAwait(false);

                var core = ExecuteCliCoreAsync(args);

                var callbacks = _parallelCallbacks.Select(a => a(cancellationToken));

                await Task.WhenAny(new[] { core }.Concat(callbacks));

                var result = core.Result;

                return result;
            }
            catch (OperationCanceledException)
            {
                return StatusCodes.Cancel;
            }
        }

        public async Task<int> ExecuteCliCoreAsync(string[] args)
        {
            if (Services == null)
            {
                throw new ArgumentNullException($"{nameof(Services)}", $"The App is not properly built: cannot execute app.");
            }

            if (RootCommand == null)
            {
                RootCommand = CommandHelper.GetRootCommand();
            }

            try
            {
                var parserResult = await RootCommand.InvokeAsync(args).ConfigureAwait(false);

                return parserResult;
            }
            catch (OperationCanceledException e)
            {
                var logger = DICore.App?.Services?.GetService<ILogger>();

                logger?.LogDebug(e, "Command was cancelled.");

                return StatusCodes.Cancel;
            }
            catch (Exception e)
            {
                var logger = DICore.App?.Services?.GetService<ILogger>();

                logger?.LogError(e, "Command execution failed.");

                return StatusCodes.Error;
            }
        }

        public void ConfigureCli(Action<RootCommand> callback)
        {
            if (callback == null)
                return;

            var root = new RootCommand();

            callback(root);

            RootCommand = root;
        }

        /// <inheritdoc />
        protected override void OnUnobservedTaskException(object sender, [NotNull] UnobservedTaskExceptionEventArgs args)
        {
            base.OnUnobservedTaskException(sender, args);

            args.Exception?.Handle(a =>
                      {
                          if (a is OperationCanceledException)
                          {
                              var statusCode = StatusCodes.Cancel;

                              OnExit(statusCode);

                              return true;
                          }
                          else
                          {
                              var statusCode = StatusCodes.Error;

                              OnExit(statusCode);

                              System.Environment.FailFast("Unobserved exception", a);
                          }

                          return false;
                      });
        }

        /// <inheritdoc />
        protected override void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            base.OnAppDomainUnhandledException(sender, args);

            if (args.ExceptionObject is OperationCanceledException)
            {
                var statusCode = StatusCodes.Cancel;

                OnExit(statusCode);
            }
            else if (args.ExceptionObject is AggregateException ag)
            {
                ag.Handle(a =>
                          {
                              if (a is OperationCanceledException)
                              {
                                  var statusCode = StatusCodes.Cancel;

                                  OnExit(statusCode);

                                  return true;
                              }

                              System.Environment.FailFast("Unobserved exception", a);

                              return false;
                          });
            }
            else
            {
                var statusCode = StatusCodes.Error;

                OnExit(statusCode);

                System.Environment.FailFast("Unobserved exception", args.ExceptionObject as Exception);
            }
        }

        public RootCommand RootCommand { get; private set; }

        public static Task RunAsync(string[] args)
        {
            var app = new CliApp();

            return app.ExecuteCliAsync(args);
        }

        [NotNull]
        [ItemNotNull]
        List<Func<CancellationToken,Task<int>>> _parallelCallbacks = new List<Func<CancellationToken, Task<int>>>();

        public void RegisterParallelCallback([NotNull] Func<CancellationToken,Task<int>> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            _parallelCallbacks.Add((ct) => Task.Run(() => callback(ct), ct));
        }

        public void RegisterParallelCallback([NotNull] Func<Task<int>> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            _parallelCallbacks.Add((ct) => Task.Run(callback, ct));
        }

        public void RegisterParallelCallback([NotNull] Func<CancellationToken, Task> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            _parallelCallbacks.Add(async (ct) =>
                                   {
                                       await Task.Run(() => callback(ct), ct);
                                       return StatusCodes.Success;
                                   });
        }

        public void RegisterParallelCallback([NotNull] Func<Task> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            _parallelCallbacks.Add( async(ct) =>
                                   {
                                       await Task.Run(callback, ct);
                                       return StatusCodes.Success;
                                   });
        }

        public void RegisterCancelKeyHandler([NotNull] Func<ConsoleKeyInfo, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            RegisterParallelCallback(async () =>
                                     {
                                         var source = Services.GetService<IProgramCancellationSource>();

                                         if (source == null)
                                         {
                                             DICore.Logger?.LogError("Cancel key handler cannot execute: {TypeName} is not registered.", nameof(IProgramCancellationSource));
                                             return (1);
                                         }

                                         do
                                         {
                                             ConsoleKeyInfo read;

                                             if (Console.KeyAvailable)
                                             {
                                                 read = Console.ReadKey(true);
                                             }

                                             if (predicate(read))
                                             {
                                                 source.Cancel();
                                                 DICore.Logger?.LogInformation("Cancel key handler: cancel requested.");
                                                 return (2);
                                             }

                                             await Task.Delay(100);
                                         } while (true);
                                     });
        }
    }
}
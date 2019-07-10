// -----------------------------------------------------------------------
//  <copyright file="AppConsoleCore.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup.Cli
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine;
    using System.CommandLine.Invocation;
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

    public class CliApp : AppCore
    {
        /// <inheritdoc />
        protected override void BuildApp([NotNull] IApplicationBuilder appBuilder, string[] args)
        {
            if (appBuilder == null)
                throw new ArgumentNullException(nameof(appBuilder));

            appBuilder.AddCliCommands();
        }

        public virtual void OnExit(bool success)
        {
            Console.WriteLine();
            Console.WriteLine();
            if (Environment.IsDevelopment())
            {
                if (!success)
                {
                    ConsoleWriter.WriteError(errorValue: "Program execution failed.");
                    Console.WriteLine();
                }

                Console.WriteLine(value: " Press any key to exit the application...");
                Console.ReadKey();
            }
            else if (!Environment.IsDevelopment() && !success)
            {
                ConsoleWriter.WriteError(errorValue: "Program execution failed.");
                Console.WriteLine();
            }
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
            if (_parallelCallbacks.Count == 0)
                return await ExecuteCliCoreAsync(args, cancellationToken);

            var core = ExecuteCliCoreAsync(args, cancellationToken);
            var callbacks = _parallelCallbacks.Select(a => a());

            await Task.WhenAny(new [] {core}.Concat(callbacks));

            var result = core.Result;

            return result;
        }

        public async Task<int> ExecuteCliCoreAsync(string[] args, CancellationToken cancellationToken = default)
        {
            if (Services == null)
            {
                throw new ArgumentNullException($"{nameof(Services)}", $"The App is not properly built: cannot execute app.");
            }

            if (RootCommand == null)
            {
                RootCommand = CommandHelper.GetRootCommand();
            }

            var parserResult = await RootCommand.InvokeAsync(args);

            return parserResult;
        }

        public void ConfigureCli(Action<RootCommand> callback)
        {
            if (callback == null)
                return;

            var root = new RootCommand();

            callback(root);

            RootCommand = root;
        }

        public async Task TerminateAsync(int code)
        {
            // waiting for logging to finish writing data
            await Task.Delay(500);

            OnExit(code.IsAnyEqual(0, 2));

            System.Environment.Exit(code);
        }

        /// <inheritdoc />
        protected override void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {

            if (args.ExceptionObject is OperationCanceledException)
            {
                ConsoleWriter.WriteError("Program cancelled.");
                TerminateAsync(2).Wait();
                return;
            }

            if (args.ExceptionObject is AggregateException ag)
            {
                ag.Handle(a =>
                          {
                              if (a is OperationCanceledException)
                              {
                                  ConsoleWriter.WriteError("Program cancelled.");
                                  TerminateAsync(2).Wait();
                                  return true;
                              }

                              return false;
                          });
            }

            base.OnAppDomainUnhandledException(sender, args);
        }

        public RootCommand RootCommand { get; private set; }

        public static Task RunAsync(string[] args)
        {
            var app = new CliApp();

            return app.ExecuteCliAsync(args);
        }

        [NotNull]
        [ItemNotNull]
        List<Func<Task<int>>> _parallelCallbacks = new List<Func<Task<int>>>();

        public void RegisterParallelCallback([NotNull] Func<Task<int>> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            _parallelCallbacks.Add(callback);
        }

        public void RegisterCancelKeyHandler([NotNull] Func<ConsoleKeyInfo, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            RegisterParallelCallback(() =>
                                     {
                                         var source = Services.GetService<IProgramCancellationSource>();

                                         if (source == null)
                                         {
                                             DICore.Logger?.LogErrorSource($"Cancel key handler cannot execute: {nameof(IProgramCancellationSource)} is not registered.");
                                             return Task.FromResult(1);
                                         }

                                         do
                                         {
                                             var read = Console.ReadKey(true);

                                             if (predicate(read))
                                             {
                                                 source.Cancel();
                                                 DICore.Logger?.LogErrorSource($"Cancel key handler: cancel requested.");
                                                 return Task.FromResult(2);
                                             }
                                         } while (true);
                                     });
        }
    }
}
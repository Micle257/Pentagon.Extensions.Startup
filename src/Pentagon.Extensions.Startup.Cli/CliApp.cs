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
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Threading;

    public class CliApp : ICliHostedService
    {
        readonly ILogger<CliApp> _logger;

        [NotNull]
        readonly IApplicationArguments _arguments;

        [NotNull]
        readonly IApplicationEnvironment _environment;

        [NotNull]
        readonly IApplicationVersion _version;

        [NotNull]
        readonly IConfiguration _configuration;

        [NotNull]
        readonly IServiceScopeFactory _serviceScopeFactory;

        [NotNull]
        readonly IHostApplicationLifetime _applicationLifetime;

        public CliApp(ILogger<CliApp> logger,
                [NotNull] IApplicationArguments arguments,
                      [NotNull] IApplicationEnvironment environment,
                      [NotNull] IApplicationVersion version,
                      [NotNull] IConfiguration configuration,
                      [NotNull] IServiceScopeFactory serviceScopeFactory,
                      [NotNull] IHostApplicationLifetime applicationLifetime)
        {
            _logger = logger;
            _arguments = arguments;
            _environment = environment;
            _version = version;
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
            _applicationLifetime = applicationLifetime;
            _scope = serviceScopeFactory.CreateScope();
        }

        public int? ResultCode { get; private set; }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (IsConsole && string.IsNullOrWhiteSpace(_environment.ApplicationName))
                    Console.Title = _environment.ApplicationName;

                var resultCode = await ExecuteCliCoreAsync(_arguments.Arguments).ConfigureAwait(false);

                ResultCode = resultCode;
            }
            catch (OperationCanceledException)
            {
                ResultCode = StatusCodes.Cancel;
            }
        }

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            OnExit(ResultCode.GetValueOrDefault());
        }

        bool IsConsole => System.Environment.UserInteractive;

        public virtual void OnExit(int statusCode)
        {
            if (IsConsole)
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
        }

        public void UpdateOptions<TOptions>([CanBeNull] Action<TOptions> updateCallback)
        {
            UpdateOptions(Options.DefaultName, updateCallback);
        }

        public void UpdateOptions<TOptions>(string name, [CanBeNull] Action<TOptions> updateCallback)
        {
            var options = _scope.ServiceProvider.GetServices<ICliOptionsSource<TOptions>>()
                                  .FirstOrDefault(a => a.Name == name);

            if (options == null)
                return;

            updateCallback?.Invoke(options.Options);

            options.Reload();
        }

        public async Task<int> ExecuteCliCoreAsync(string[] args)
        {
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
        protected void OnUnobservedTaskException(object sender, [NotNull] UnobservedTaskExceptionEventArgs args)
        {
            _logger.LogError(exception: args.Exception, message: "Exception unhandled (TaskScheduler).");

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
        protected void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            _logger.LogError(args.ExceptionObject as Exception,
                                                     message: "Exception unhandled (AppDomain).");

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

        IServiceScope _scope;
    }
}
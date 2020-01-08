// -----------------------------------------------------------------------
//  <copyright file="CliHostedService.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup.Cli
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Console;
    using Console.Cli;
    using JetBrains.Annotations;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class CliHostedService : ICliHostedService
    {
        readonly ILogger<CliHostedService> _logger;

        [NotNull]
        readonly IApplicationArguments _arguments;

        [NotNull]
        readonly ICliCommandRunner _cliRunner;

        [NotNull]
        readonly IHostEnvironment _environment;

        [NotNull]
        readonly IHostApplicationLifetime _lifetime;

        readonly IServiceScope _scope;

        public CliHostedService(ILogger<CliHostedService> logger,
                                [NotNull] IApplicationArguments arguments,
                                [NotNull] ICliCommandRunner cliRunner,
                                [NotNull] IHostEnvironment environment,
                                [NotNull] IHostApplicationLifetime lifetime,
                                [NotNull] IServiceScopeFactory serviceScopeFactory)
        {
            _logger      = logger;
            _arguments   = arguments;
            _cliRunner   = cliRunner;
            _environment = environment;
            _lifetime = lifetime;
            _scope       = serviceScopeFactory.CreateScope();
        }

        public int? ResultCode { get; private set; }

        bool IsConsole => Environment.UserInteractive;

        /// <inheritdoc />
        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
                TaskScheduler.UnobservedTaskException      += OnUnobservedTaskException;

                if (IsConsole && !string.IsNullOrWhiteSpace(_environment.ApplicationName))
                    Console.Title = _environment.ApplicationName; // TODO some cli options

                var resultCode = await ExecuteCliCoreAsync(_arguments.Arguments, stoppingToken).ConfigureAwait(false);

                ResultCode = resultCode;
            }
            catch (OperationCanceledException)
            {
                ResultCode = StatusCodes.Cancel;
            }

            OnExit(ResultCode.GetValueOrDefault());

            _lifetime.StopApplication();
        }

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
            }
        }

        public async Task<int> ExecuteCliCoreAsync(string[] args, CancellationToken cancellationToken)
        {
            try
            {
                var parserResult = await _cliRunner.RunAsync(args, cancellationToken).ConfigureAwait(false);

                return parserResult;
            }
            catch (OperationCanceledException e)
            {
                _logger?.LogDebug(e, "Command was cancelled.");

                return StatusCodes.Cancel;
            }
            catch (Exception e)
            {
                _logger?.LogError(e, "Command execution failed.");

                return StatusCodes.Error;
            }
        }

        /// <inheritdoc />
        protected void OnUnobservedTaskException(object sender, [NotNull] UnobservedTaskExceptionEventArgs args)
        {
            _logger.LogError(args.Exception, "Exception unhandled (TaskScheduler).");

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

                                           Environment.FailFast("Unobserved exception", a);
                                       }

                                       return false;
                                   });
        }

        /// <inheritdoc />
        protected void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            _logger.LogError(args.ExceptionObject as Exception,
                             "Exception unhandled (AppDomain).");

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

                              Environment.FailFast("Unobserved exception", a);

                              return false;
                          });
            }
            else
            {
                var statusCode = StatusCodes.Error;

                OnExit(statusCode);

                Environment.FailFast("Unobserved exception", args.ExceptionObject as Exception);
            }
        }
    }
}
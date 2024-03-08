using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;

var services = 
    new ServiceCollection()
        .AddSingleton(new LoggingLevelSwitch())
        .AddSingleton<ILogger>(sp =>
            {
                var logLevelSwitch = sp.GetRequiredService<LoggingLevelSwitch>();
                return new LoggerConfiguration()
                    .MinimumLevel.ControlledBy(logLevelSwitch).WriteTo
                    .Console()
                    .CreateLogger();
            });

var helloCommand = new Command("hello");
helloCommand.Handler = CommandHandler.Create<ILogger>(async (logger) =>
{
    logger.Information("INFO");
    logger.Debug("DBG");
    return await Task.FromResult(0);
});

var rootCommand = new RootCommand { helloCommand };
var verboseOption = new Option<bool>(new[] { "--verbose", "-v" }, "Lowers minimum logging level to debug");
rootCommand.AddGlobalOption(verboseOption);
var builder = 
    new CommandLineBuilder(rootCommand)
        .UseDefaults()
        .AddMiddleware(async (context, next) =>
        {
            await using var serviceProvider = services.BuildServiceProvider();
            context.BindingContext.AddService<IServiceProvider>(_ => serviceProvider);
            foreach(var serviceType in services.Select(_ => _.ServiceType))
                context.BindingContext.AddService(
                    serviceType, 
                    _ => serviceProvider.GetRequiredService(serviceType));
            await next(context);
        })
        .AddMiddleware(async (context, next) =>
        {
            var logSwitch =
                context
                    .BindingContext
                    .GetRequiredService<IServiceProvider>()
                    .GetRequiredService<LoggingLevelSwitch>();
            var configureLogLevel = context.ParseResult.HasOption(verboseOption)
                ? new Action(() => logSwitch.MinimumLevel = LogEventLevel.Verbose)
                : new Action(() => logSwitch.MinimumLevel = LogEventLevel.Information);
            configureLogLevel();
            await next(context);
        });
await builder.Build().InvokeAsync("hello");
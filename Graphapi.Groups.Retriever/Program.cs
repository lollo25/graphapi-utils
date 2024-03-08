using Graphapi.Groups.Retriever.Models;
using Graphapi.Groups.Retriever.Services;
using LanguageExt.UnsafeValueAccess;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using Constants = Graphapi.Groups.Retriever.Constants;

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
            })
        .AddSingleton<IAuthorizationProvider, GraphApiAuthorizationProvider>();
services
    .AddHttpClient(Constants.MicrosoftLoginClient, sp => sp.BaseAddress = new Uri("https://login.microsoftonline.com"));

var helloCommand = new Command("download-groups");
helloCommand.Handler = CommandHandler.Create<AuthenticationOptions, IServiceProvider, CancellationToken>(async (ao, sp, ct) =>
{
    var logger = sp.GetRequiredService<ILogger>();
    logger.Information("INFO " + ao.Tenant);
    var service = ActivatorUtilities.GetServiceOrCreateInstance<IAuthorizationProvider>(sp)!;
    var res = await service.AuthenticateAsync(ao, ct);
    logger.Information("AT: " + res.ValueUnsafe().AccessToken);
    return await Task.FromResult(0);
});

var rootCommand = new RootCommand { helloCommand };
var verboseOption = new Option<bool>(new[] { "--verbose", "-v" }, "Lowers minimum logging level to debug");
var tenantOption = new Option<string>(new[] { "--tenant", "-t" }, "The directory tenant that you want to request permission from. The value can be in GUID or a friendly name format.");
var appIdOption = new Option<string>(new[] { "--app-id", "-aid" }, "The application ID that the Azure app registration portal assigned when you registered your app");
var secretOption = new Option<string>(new[] { "--app-secret", "-as" }, "The client secret that you generated for your app in the app registration portal.");
rootCommand.AddGlobalOption(verboseOption);
rootCommand.AddGlobalOption(tenantOption);
rootCommand.AddGlobalOption(appIdOption);
rootCommand.AddGlobalOption(secretOption);
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
await builder.Build().InvokeAsync(args);
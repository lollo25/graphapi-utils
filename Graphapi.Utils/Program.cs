using Graphapi.Utils;
using Graphapi.Utils.Models;
using Graphapi.Utils.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.IO.Abstractions;
using Constants = Graphapi.Utils.Constants;

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
        .AddSingleton<IAuthorizationProvider, GraphApiAuthorizationProvider>()
        .Decorate<IAuthorizationProvider, LoggedAuthorizationProvider>()
        .AddSingleton(typeof(IListDownloader<>), typeof(ListDownloader<>))
        .AddSingleton(typeof(IGraphApiClient<>), typeof(GraphApiClient<>))
        .AddSingleton(typeof(IListFileSystemSaver<>), typeof(ListFileSystemSaver<>))
        .AddSingleton<IFileSystem, FileSystem>();

services
    .AddHttpClient(
        Constants.MicrosoftLoginClient,
        sp => sp.BaseAddress = new Uri(Constants.LoginApiRootUrl));
services
    .AddHttpClient(
        Constants.GraphApiClient);

var pageSizeOption = new Option<int>(
    new[] { "--page-size" },
    getDefaultValue: () => Constants.DefaultPageSize,
    "The page size of each request.");
var toDirOption = new Option<string>(
    new[] { "--directory", "-dir" },
    getDefaultValue: () => "/MSGraph/Groups",
    "The directory where the items will be saved into.")
{
    IsRequired = true
};
var groupsRetrieveCommand = new Command("download-groups");
groupsRetrieveCommand.AddOption(pageSizeOption);
groupsRetrieveCommand.AddOption(toDirOption);
groupsRetrieveCommand.Handler = CommandHandler.Create<ListDownloaderOptions, IServiceProvider, CancellationToken>(async (o, sp, ct) =>
{
    var downloader = sp.GetRequiredService<IListDownloader<GraphApiGroup>>();
    var saver = sp.GetRequiredService<IListFileSystemSaver<GraphApiGroup>>();
    var logger = sp.GetRequiredService<ILogger>();
    await downloader.DownloadAsync("groups", o, ResiliencePipelines.RetryOnThrottle<GraphApiGroup>(logger), ct)
        .Bind(_ => saver.SaveAsync(_, g => $"{g.DisplayName}.json", o))
        .Match(
            list =>
            {
                logger.Information($"Groups are {list.Length()}");
                foreach (var item in list)
                {
                    logger.Information($"Group saved to {item}");
                }
            },
            err => logger.Error($"Error: {err.Message}"));
    return await Task.FromResult(0);
});

var rootCommand = new RootCommand { groupsRetrieveCommand };
var verboseOption = new Option<bool>(
    new[] { "--verbose", "-v" },
    "Lowers minimum logging level to debug");
var tenantOption = new Option<string>(
    new[] { "--tenant", "-t" },
    "The directory tenant that you want to request permission from. The value can be in GUID or a friendly name format.")
{
    IsRequired = true
};
var appIdOption = new Option<string>(
    new[] { "--app-id", "-aid" },
    "The application ID that the Azure app registration portal assigned when you registered your app")
{
    IsRequired = true
};
var secretOption = new Option<string>(
    new[] { "--app-secret", "-as" },
    "The client secret that you generated for your app in the app registration portal.")
{
    IsRequired = true
};
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
            foreach (var serviceType in services.Select(_ => _.ServiceType))
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
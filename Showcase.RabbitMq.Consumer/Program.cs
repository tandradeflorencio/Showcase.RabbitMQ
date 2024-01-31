using Serilog;
using Showcase.RabbitMq.Consumer;
using Showcase.RabbitMq.Consumer.Configurations;
using Showcase.RabbitMq.Consumer.Utils;

const string LogPrefix = $"{ConstantsUtil.LogPrefix}";

try
{
    Log.Information($"{LogPrefix} {nameof(Program)} The application is starting.");

    IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(hostContext.HostingEnvironment.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables();

        IConfiguration configuration = builder.Build();

        services.ConfigureLogs();
        services.AddHostedService<Worker>();
    })
    .UseSerilog((hostingContext, loggerConfig) => loggerConfig
        .ReadFrom.Configuration(hostingContext.Configuration)
        .Enrich.WithProperty(ConstantsUtil.LogPrefix, ConstantsUtil.LogPrefix)
        .WriteTo.ApplicationInsights(hostingContext?.Configuration.GetConnectionString("ApplicationInsights"), TelemetryConverter.Events)
    )
    .Build();

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, $"{LogPrefix} {nameof(Program)} Fatal error. The application is shutting down.");
}
finally
{
    Log.Information($"{LogPrefix} {nameof(Program)} The application is shutting down.");
    Log.CloseAndFlush();
}
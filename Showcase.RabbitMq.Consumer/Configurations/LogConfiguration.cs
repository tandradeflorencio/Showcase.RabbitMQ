using Serilog;

namespace Showcase.RabbitMq.Consumer.Configurations
{
    internal static class LogConfiguration
    {
        public static void ConfigureLogs(this IServiceCollection services)
        {
            services.AddSingleton(Log.Logger);
        }
    }
}
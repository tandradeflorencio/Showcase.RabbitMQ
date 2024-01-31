using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using Showcase.RabbitMq.Producer.Settings;
using System.Text;

const string QueueName = "RabbitMqExampleQueue";
const int MessagesCount = 10;

var logger = new LoggerConfiguration()
    .WriteTo.Console(theme: AnsiConsoleTheme.Literate)
    .CreateLogger();

try
{
    logger.Information($"Sending messages to RabbitMQ {QueueName} queue.");

    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false);

    IConfiguration configuration = builder.Build();

    var rabbiMqSettings = configuration.GetSection("RabbitMq").Get<RabbitMqSettings>();

    if (rabbiMqSettings == null)
    {
        logger.Information($"RabbitMQ configuration not found. No messages sent.");
        return;
    }

    var factory = new ConnectionFactory
    {
        HostName = rabbiMqSettings.HostName,
        UserName = rabbiMqSettings.Username,
        Password = rabbiMqSettings.Password,
    };

    using (var connection = factory.CreateConnection())
    using (var model = connection.CreateModel())
    {
        model.QueueDeclare(queue: QueueName,
                           durable: false,
                           exclusive: false,
                           autoDelete: false,
                           arguments: null);

        for (int index = 0; index < MessagesCount; index++)
        {
            var message = Guid.NewGuid().ToString();

            model.BasicPublish(exchange: string.Empty,
                               routingKey: QueueName,
                               basicProperties: null,
                               body: Encoding.UTF8.GetBytes(message));
        }
    }

    logger.Information($"Messages sent to RabbitMQ {QueueName} queue.");
}
catch (Exception ex)
{
    logger.Error($"An error ocurred: {ex.Message}. Please, try again.");
}
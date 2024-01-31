using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Showcase.RabbitMq.Consumer
{
    public class Worker : BackgroundService
    {
        private const string QueueName = "RabbitMqExampleQueue";

        private readonly Serilog.ILogger _logger;
        private readonly IConfiguration _configuration;

        public Worker(Serilog.ILogger logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Information($"Receiving messages from RabbitMQ {QueueName} queue.");

            var rabbiMqConnection = _configuration.GetConnectionString("RabbitMq");

            if (rabbiMqConnection == null)
            {
                _logger.Information($"RabbitMQ configuration not found. No messages sent.");
                return;
            }

            var factory = new ConnectionFactory
            {
                Uri = new Uri(rabbiMqConnection)
            };

            using var connection = factory.CreateConnection();
            using var model = connection.CreateModel();

            model.QueueDeclare(queue: QueueName,
                               durable: false,
                               exclusive: false,
                               autoDelete: false,
                               arguments: null);

            var consumer = new EventingBasicConsumer(model);

            consumer.Received += Consumer_Received;

            model.BasicConsume(queue: QueueName,
                                autoAck: true,
                                consumer: consumer);

            var workerActiveIntervalInMilliseconds = _configuration.GetValue<int>("WorkerActiveIntervalInMilliseconds");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.Information($"Worker actived at {DateTime.Now:yyyy-MM-dd HH:mm:ss}.");

                await Task.Delay(workerActiveIntervalInMilliseconds, stoppingToken);
            }
        }

        private void Consumer_Received(object? sender, BasicDeliverEventArgs e)
        {
            _logger.Information($"Message received at {DateTime.Now:yyyy-MM-dd HH:mm:ss}. {Encoding.UTF8.GetString(e.Body.ToArray())}");
        }
    }
}
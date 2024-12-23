using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQModel = RabbitMQ.Client.IModel;
using ServiLogs.Application.Models;
using ServiLogs.Application.Services;
using System.Text;
using System.Text.Json;
using ServiLogs.Infrastructure.RabbitMQ;

namespace ServiLogs.Infrastructure.Events
{
    public class RabbitMQConsumer : BackgroundService
    {
        private readonly RabbitMQConfiguration _config;
        // IServiceScopeFactory: Permite crear un alcance de servicios (scope)
        // para resolver dependencias en tiempo de ejecución (como ILogService
        private readonly IServiceScopeFactory _serviceScopeFactory;

        // IConnection y RabbitMQModel(desambiguado): Proporcionados por el cliente de RabbitMQ,
        // representan una conexión al servidor RabbitMQ y un canal para enviar/recibir
        // mensajes, respectivamente.
        private IConnection _connection;
        private RabbitMQModel _channel;

        // Agregados para stop y run
        // ManualResetEventSlim: Un mecanismo de sincronización que permite
        // pausar y reanudar el consumo de mensajes.
        private readonly ManualResetEventSlim _pauseEvent = new ManualResetEventSlim(true);

        public RabbitMQConsumer(RabbitMQConfiguration config, IServiceScopeFactory serviceScopeFactory)
        {
            _config = config;
            _serviceScopeFactory = serviceScopeFactory;

            // Crear la conexion
            var factory = new ConnectionFactory
            {
                HostName = _config.HostName,
                UserName = _config.UserName,
                Password = _config.Password
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Asegurar que la cola exista
            _channel.QueueDeclare(queue: _config.QueueName,
                durable: true, // Indica que la cola persiste luego de  reiniciar el servidor
                exclusive: false, // Indica si la cola es exclusiva para la conexion
                autoDelete: false, // Indica si la cola se elimina automaticamente si no hay consumidores
                arguments: null);
        }

        public void StopConsume() => _pauseEvent.Reset();
        public void RunConsume() => _pauseEvent.Set();

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Escuchar mensajes de la cola definida
            var consumer = new EventingBasicConsumer(_channel);

            // Ejecucion por cada mensaje que se escucha
            consumer.Received += async (model, ea) =>
            {

                _pauseEvent.Wait(stoppingToken); // Detiene la ejecucion si fue stopped

                var body = ea.Body.ToArray(); // Convierte el mensaje en bytes
                var message = Encoding.UTF8.GetString(body);

                var logEntry = JsonSerializer.Deserialize<LogEntry>(message); // Desearializa el mensaje
                if (logEntry != null)
                {

                    using (var scope = _serviceScopeFactory.CreateScope()) // Crea el ambito
                    {
                        var logService = scope.ServiceProvider.GetRequiredService<ILogService>(); // Recupera el logService (dinamico)
                        await logService.RegisterLogAsync(logEntry); // Ejecuta logService
                    }
                }

                _channel.BasicAck(ea.DeliveryTag, false); // Notificar al servidor Rabbit que borre el mensaje
            };


            // Configura al servidor para que escuche de forma indefinida
            _channel.BasicConsume(queue: _config.QueueName, autoAck: false, consumer: consumer);

            return Task.CompletedTask;
        }

        // Cierra el canal y la conexion cuando el servicio se detiene o se elimina
        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}

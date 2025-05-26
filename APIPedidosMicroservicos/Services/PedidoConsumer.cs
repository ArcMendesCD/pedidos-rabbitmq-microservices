using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace APIPedidosMicroservicos.Services
{
    public class PedidoConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private IConnection _connection;
        private IModel _channel;

        private const string ExchangeName = "pedidos_topic_exchange";
        private const string QueueName = "pagamentos_queue";
        private const string RoutingKey = "pagamento.confirmado";

        public PedidoConsumer(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

            var uri = new Uri("amqps://xzhfkjrz:Kq0gWWMiMN5xR9m1Rmz1kE89Go8ALf4h@porpoise.rmq.cloudamqp.com/xzhfkjrz");

            var factory = new ConnectionFactory()
            {
                Uri = uri,
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Topic, durable: true);
            _channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(queue: QueueName, exchange: ExchangeName, routingKey: RoutingKey);

            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("[PedidoConsumer] Consumindo mensagem...");
    
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += async (sender, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                Console.WriteLine($"[PedidoConsumer] Mensagem recebida: {message}");

                try
                {
                    var pagamentoConfirmado = JsonSerializer.Deserialize<PagamentoConfirmado>(message);

                    if (pagamentoConfirmado == null)
                    {
                        Console.WriteLine("[PedidoConsumer] Mensagem desserializada como nula.");
                        _channel.BasicNack(ea.DeliveryTag, false, false);
                        return;
                    }

                    using var scope = _scopeFactory.CreateScope();
                    var pedidoService = scope.ServiceProvider.GetRequiredService<PedidoService>();

                    await pedidoService.AtualizarStatusPedidoAsync(
                        pagamentoConfirmado.PedidoId,
                        pagamentoConfirmado.StatusPagamento
                    );

                    // reconhece um pagamento bem sucedido
                    _channel.BasicAck(ea.DeliveryTag, false);
                    Console.WriteLine($"[PedidoConsumer] Confirmação de pagamento processada com sucesso PedidoId: {pagamentoConfirmado.PedidoId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PedidoConsumer] Erro processando a mensagem: {ex.Message}");
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

            // serviço roda ate ser cancelado
            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[PedidoConsumer] Saindo do consumer...");
            }
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }

    public class PagamentoConfirmado
    {
        public int PedidoId { get; set; }
        public decimal ValorPago { get; set; }
        public string StatusPagamento { get; set; }
        public DateTime DataConfirmacao { get; set; }
    }

}

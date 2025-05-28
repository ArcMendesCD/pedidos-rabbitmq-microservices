using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using PagamentoService.Entities;
using PagamentoService.Infrastructure;
using PagamentoService.Models;

namespace PagamentoService.Services
{
    public class Int32JsonConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (int.TryParse(stringValue, out int value))
                {
                    return value;
                }
                throw new JsonException($"Unable to convert \"{stringValue}\" to int.");
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt32();
            }
            else
            {
                throw new JsonException($"Unexpected token parsing int. Token: {reader.TokenType}");
            }
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }

    public class PagamentoConsumer
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IServiceScopeFactory _scopeFactory;

        private const string ExchangeName = "pedidos_topic_exchange";
        private const string ConsumingQueue = "pedidos_novos_queue";
        private const string RoutingKeyIn = "pedido.criado";
        private const string RoutingKeyOut = "pagamento.confirmado";

        public PagamentoConsumer(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

            var factory = new ConnectionFactory
            {
                Uri = new Uri(""), // URI DO RABBITMQ
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(ExchangeName, ExchangeType.Topic, durable: true);

            _channel.QueueDeclare(ConsumingQueue, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(ConsumingQueue, ExchangeName, RoutingKeyIn);


        }

        public void Start()
        {
            Console.WriteLine("[PagamentoService] Starting consumer for pedido.criado messages...");
            
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                Console.WriteLine($"[PagamentoService] Recebido: {json}");

                if (json.Contains("StatusPagamento") && json.Contains("DataConfirmacao"))
                {
                    Console.WriteLine("[PagamentoService] Ignorando mensagem de confirmação de pagamento");
                    _channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                try
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new Int32JsonConverter() }
                    };

                    var pedido = JsonSerializer.Deserialize<PedidoDTO>(json, options);
                    if (pedido == null)
                    {
                        Console.WriteLine("[PagamentoService] Erro: Pedido desserializado como null.");
                        _channel.BasicNack(ea.DeliveryTag, false, false);
                        return;
                    }

                    Console.WriteLine($"[PagamentoService] Processando pedido: {pedido.PedidoId}");

                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var pagamento = new Pagamento
                    {
                        PedidoId = pedido.PedidoId,
                        ClienteId = pedido.ClienteId,
                        Valor = pedido.ValorTotal
                    };

                    dbContext.Pagamentos.Add(pagamento);
                    await dbContext.SaveChangesAsync();

                    var confirmado = new PagamentoConfirmado
                    {
                        PedidoId = pedido.PedidoId,
                        ClienteId = pedido.ClienteId,
                        ValorPago = pedido.ValorTotal,
                        StatusPagamento = "confirmado",
                        DataConfirmacao = DateTime.UtcNow
                    };

                    var msg = JsonSerializer.Serialize(confirmado);
                    var bodyOut = Encoding.UTF8.GetBytes(msg);

                    _channel.BasicPublish(
                        exchange: ExchangeName,
                        routingKey: RoutingKeyOut,
                        basicProperties: null,
                        body: bodyOut
                    );

                    Console.WriteLine($"[PagamentoService] pagamento.confirmado publicado para exchange com routing key '{RoutingKeyOut}' - PedidoId {pedido.PedidoId}");

                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PagamentoService] Erro: {ex.Message}");
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(queue: ConsumingQueue, autoAck: false, consumer: consumer);
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
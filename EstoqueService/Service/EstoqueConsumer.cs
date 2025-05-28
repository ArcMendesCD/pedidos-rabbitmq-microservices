using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EstoqueService.Models;
using EstoqueService.Entities;
using EstoqueService.Infrastructure;

namespace EstoqueService.Services
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
                throw new JsonException($"Impossivel converter \"{stringValue}\" to int.");
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt32();
            }
            else
            {
                throw new JsonException($"Json Exception: {reader.TokenType}");
            }
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }

    public class EstoqueConsumer
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IServiceScopeFactory _scopeFactory;

        private const string ExchangeName = "pedidos_topic_exchange";
        private const string ConsumingQueue = "estoque_queue";
        private const string RoutingKeyIn = "pedido.criado";
        private const string RoutingKeyOut = "estoque.reservado";

        public EstoqueConsumer(IServiceScopeFactory scopeFactory)
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
            Console.WriteLine("[EstoqueService] Iniciando consumidor de pedido.criado...");

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

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
                        Console.WriteLine("[EstoqueService] Erro: Pedido desserializado como null.");
                        _channel.BasicNack(ea.DeliveryTag, false, false);
                        return;
                    }

                    Console.WriteLine($"[EstoqueService] Processando reserva para pedido: {pedido.PedidoId}");

                    // converte DTO para entidade
                    var itensReserva = pedido.Itens.Select(i => new ReservaItem
                    {
                        ProdutoId = i.ProdutoId.ToString(),
                        Quantidade = i.Quantidade
                    }).ToList();

                    var reserva = new ReservaEstoque
                    {
                        PedidoId = pedido.PedidoId.ToString(),
                        Itens = itensReserva,
                        StatusEstoque = "reservado",
                        DataReserva = DateTime.UtcNow
                    };

                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    dbContext.Reservas.Add(reserva);
                    await dbContext.SaveChangesAsync();

                    // converte entidade para DTO
                    var evento = new EstoqueReservadoDTO
                    {
                        PedidoId = reserva.PedidoId,
                        StatusEstoque = reserva.StatusEstoque,
                        DataReserva = reserva.DataReserva,
                        ItensReservados = reserva.Itens.Select(i => new ReservaItemDTO
                        {
                            ProdutoId = i.ProdutoId,
                            Quantidade = i.Quantidade
                        }).ToList()
                    };

                    var msg = JsonSerializer.Serialize(evento);
                    var bodyOut = Encoding.UTF8.GetBytes(msg);

                    _channel.BasicPublish(
                        exchange: ExchangeName,
                        routingKey: RoutingKeyOut,
                        basicProperties: null,
                        body: bodyOut
                    );

                    Console.WriteLine($"[EstoqueService] estoque.reservado publicado para PedidoId {reserva.PedidoId}");

                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[EstoqueService] Erro: {ex.Message}");
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

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using NotificacaoService.Models;

namespace NotificacaoService.Services;

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
            throw new JsonException($"Não foi possível converter \"{stringValue}\" para int.");
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetInt32();
        }
        else
        {
            throw new JsonException($"Tipo inválido para conversão de int: {reader.TokenType}");
        }
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}
public class NotificacaoConsumer : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    private const string ExchangeName = "pedidos_topic_exchange";
    private const string QueueName = "notificacao_queue";
    private const string RoutingKey = "pagamento.confirmado";

    public NotificacaoConsumer()
    {
        var factory = new ConnectionFactory
        {
            Uri = new Uri(""), // URI DO RABBITMQ
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(ExchangeName, ExchangeType.Topic, durable: true);
        _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(QueueName, ExchangeName, RoutingKey);
    }

    public void Start()
    {
        Console.WriteLine("[NotificacaoService] Aguardando mensagens de pagamento.confirmado...");

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);

            try
            {
                var dto = JsonSerializer.Deserialize<PagamentoConfirmadoDTO>(json);
                if (dto != null)
                {
                    Console.WriteLine($"[NotificacaoService] Email enviado para cliente ID: {dto.ClienteId}");
                }

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificacaoService] Erro: {ex.Message}");
                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}

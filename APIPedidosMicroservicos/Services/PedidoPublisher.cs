using RabbitMQ.Client;
using System;
using System.Text;
using System.Text.Json;

public class PedidoPublisher
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private const string ExchangeName = "pedidos_topic_exchange";

    public PedidoPublisher()
    {
        var uri = new Uri("amqps://xzhfkjrz:Kq0gWWMiMN5xR9m1Rmz1kE89Go8ALf4h@porpoise.rmq.cloudamqp.com/xzhfkjrz");

        var factory = new ConnectionFactory()
        {
            Uri = uri
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Topic, durable: true);
        
        string queueName = "pagamentos_queue";
        string routingKey = "pedido.criado";

        _channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
            );
        _channel.QueueBind(queue: queueName,
            exchange: ExchangeName,
            routingKey: routingKey);
    }

    public void PublicarPedidoCriado(PedidoDTO pedido)
    {
        string routingKey = "pedido.criado";
        string json = JsonSerializer.Serialize(pedido);
        var body = Encoding.UTF8.GetBytes(json);

        _channel.BasicPublish(
            exchange: ExchangeName,
            routingKey: routingKey,
            basicProperties: null,
            body: body
        );
    }
}
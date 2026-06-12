namespace Infrastructure.Messaging;

public class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string CreateOrderExchangeName { get; set; } = "arase.orders";
    public string CreateOrderQueueName { get; set; } = "arase.order.create";
    public string CreateOrderRoutingKey { get; set; } = "order.create";
}

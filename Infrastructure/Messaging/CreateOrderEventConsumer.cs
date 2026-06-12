using System.Text.Json;
using Application.DTOs.Orders;
using Application.Interfaces;
using Application.Messaging.Orders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Infrastructure.Messaging;

public class CreateOrderEventConsumer : BackgroundService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<CreateOrderEventConsumer> _logger;
    private readonly RabbitMqOptions _options;
    private IConnection? _connection;
    private IChannel? _channel;

    public CreateOrderEventConsumer(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<CreateOrderEventConsumer> logger,
        IOptions<RabbitMqOptions> options)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ConnectAsync(stoppingToken);
        await ConfigureTopologyAsync(stoppingToken);
        await StartConsumerAsync(stoppingToken);

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null)
        {
            await _channel.CloseAsync(cancellationToken);
        }

        if (_connection is not null)
        {
            await _connection.CloseAsync(cancellationToken);
        }

        await base.StopAsync(cancellationToken);
    }

    private async Task ConnectAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        _logger.LogInformation("Connected to RabbitMQ for create order events.");
    }

    private async Task ConfigureTopologyAsync(CancellationToken cancellationToken)
    {
        var channel = GetChannel();

        await channel.ExchangeDeclareAsync(
            _options.CreateOrderExchangeName,
            ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            _options.CreateOrderQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            _options.CreateOrderQueueName,
            _options.CreateOrderExchangeName,
            _options.CreateOrderRoutingKey,
            cancellationToken: cancellationToken);

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false,
            cancellationToken: cancellationToken);
    }

    private async Task StartConsumerAsync(CancellationToken cancellationToken)
    {
        var channel = GetChannel();
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += ProcessMessageAsync;

        await channel.BasicConsumeAsync(
            _options.CreateOrderQueueName,
            autoAck: false,
            consumer,
            cancellationToken);

        _logger.LogInformation("Create order RabbitMQ consumer started.");
    }

    private async Task ProcessMessageAsync(object sender, BasicDeliverEventArgs eventArgs)
    {
        var channel = GetChannel();

        try
        {
            var message = JsonSerializer.Deserialize<CreateOrderEvent>(
                eventArgs.Body.Span,
                JsonSerializerOptions);

            if (message is null)
            {
                throw new InvalidOperationException("CreateOrderEvent message body was empty or invalid.");
            }

            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
            var input = new AddOrderInDto
            {
                TrackingId = message.TrackingId,
                CustomerId = message.CustomerId,
                Side = message.Side,
                Amount = message.Amount
            };

            await orderService.AddOrderAsync(input, CancellationToken.None);
            await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);

            _logger.LogInformation(
                "CreateOrderEvent processed successfully. TrackingId: {TrackingId}",
                message.TrackingId);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "CreateOrderEvent processing failed. DeliveryTag: {DeliveryTag}",
                eventArgs.DeliveryTag);

            await channel.BasicNackAsync(
                eventArgs.DeliveryTag,
                multiple: false,
                requeue: true);
        }
    }

    private IChannel GetChannel()
    {
        return _channel ?? throw new InvalidOperationException("RabbitMQ channel has not been initialized.");
    }
}

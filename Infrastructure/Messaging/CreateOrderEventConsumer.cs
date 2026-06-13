using System.Text.Json;
using Application.DTOs.Orders;
using Application.Interfaces;
using Contracts.Events;
using Mapster;
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
        try
        {
            if (_channel is not null)
            {
                await _channel.CloseAsync(cancellationToken);
                await _channel.DisposeAsync();
            }

            if (_connection is not null)
            {
                await _connection.CloseAsync(cancellationToken);
                await _connection.DisposeAsync();
            }
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Error while closing RabbitMQ create order consumer resources.");
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

        await channel.ExchangeDeclareAsync(
            _options.CreateOrderDeadLetterExchangeName,
            ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            _options.CreateOrderDeadLetterQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            _options.CreateOrderDeadLetterQueueName,
            _options.CreateOrderDeadLetterExchangeName,
            _options.CreateOrderDeadLetterRoutingKey,
            cancellationToken: cancellationToken);

        var queueArguments = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = _options.CreateOrderDeadLetterExchangeName,
            ["x-dead-letter-routing-key"] = _options.CreateOrderDeadLetterRoutingKey
        };

        await channel.QueueDeclareAsync(
            _options.CreateOrderQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: queueArguments,
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
        consumer.ReceivedAsync += (_, eventArgs) => ProcessMessageAsync(eventArgs, cancellationToken);

        await channel.BasicConsumeAsync(
            _options.CreateOrderQueueName,
            autoAck: false,
            consumer,
            cancellationToken);

        _logger.LogInformation(
            "Create order RabbitMQ consumer started. Queue: {QueueName}, DeadLetterQueue: {DeadLetterQueueName}",
            _options.CreateOrderQueueName,
            _options.CreateOrderDeadLetterQueueName);
    }

    private async Task ProcessMessageAsync(
        BasicDeliverEventArgs eventArgs,
        CancellationToken cancellationToken)
    {
        var channel = GetChannel();

        try
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
            var message = DeserializeMessage(eventArgs.Body);
            var input = message.Adapt<AddOrderInDto>();

            await orderService.AddOrderAsync(input, cancellationToken);
            await channel.BasicAckAsync(
                eventArgs.DeliveryTag,
                multiple: false,
                cancellationToken);

            _logger.LogInformation(
                "CreateOrderEvent processed and ACK sent. TrackingId: {TrackingId}, DeliveryTag: {DeliveryTag}, Redelivered: {Redelivered}",
                message.TrackingId,
                eventArgs.DeliveryTag,
                eventArgs.Redelivered);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                "CreateOrderEvent processing cancelled. DeliveryTag: {DeliveryTag}",
                eventArgs.DeliveryTag);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "CreateOrderEvent processing failed. NACK will dead-letter the message. DeliveryTag: {DeliveryTag}, Redelivered: {Redelivered}",
                eventArgs.DeliveryTag,
                eventArgs.Redelivered);

            await channel.BasicNackAsync(
                eventArgs.DeliveryTag,
                multiple: false,
                requeue: false,
                cancellationToken);
        }
    }

    private static CreateOrderEvent DeserializeMessage(ReadOnlyMemory<byte> body)
    {
        var message = JsonSerializer.Deserialize<CreateOrderEvent>(
            body.Span,
            JsonSerializerOptions);

        return message ?? throw new InvalidOperationException("CreateOrderEvent message body was empty or invalid.");
    }

    private IChannel GetChannel()
    {
        return _channel ?? throw new InvalidOperationException("RabbitMQ channel has not been initialized.");
    }
}

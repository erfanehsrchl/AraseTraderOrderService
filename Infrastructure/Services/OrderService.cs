using Application.DTOs.Orders;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class OrderService : IOrderService
{
    private const string OrderRegisteredMessage = "Order has been registered and is pending wallet processing.";
    private readonly AppDbContext _dbContext;

    public OrderService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AddOrderOutDto> AddOrderAsync(AddOrderInDto input, CancellationToken cancellationToken)
    {
        var existingOrder = await _dbContext.Orders
            .Where(order => order.TrackingId == input.TrackingId)
            .Select(order => new AddOrderOutDto
            {
                TrackingId = order.TrackingId,
                Status = order.Status,
                Message = OrderRegisteredMessage
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (existingOrder is not null)
        {
            return existingOrder;
        }

        var customerExists = await _dbContext.Customers
            .AnyAsync(customer => customer.Id == input.CustomerId, cancellationToken);

        if (!customerExists)
        {
            throw new InvalidOperationException("Customer was not found.");
        }

        var now = DateTime.UtcNow;
        var order = new Order
        {
            TrackingId = input.TrackingId,
            CustomerId = input.CustomerId,
            Side = input.Side,
            Amount = input.Amount,
            Status = OrderStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AddOrderOutDto
        {
            TrackingId = order.TrackingId,
            Status = order.Status,
            Message = OrderRegisteredMessage
        };
    }
}

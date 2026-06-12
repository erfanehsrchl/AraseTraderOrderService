using Application.DTOs.Orders;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Mapster;
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
            .AsNoTracking()
            .Where(order => order.TrackingId == input.TrackingId)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingOrder is not null)
        {
            return existingOrder.Adapt<AddOrderOutDto>();
        }

        var customerExists = await _dbContext.Customers
            .AnyAsync(customer => customer.Id == input.CustomerId, cancellationToken);

        if (!customerExists)
        {
            throw new InvalidOperationException("Customer was not found.");
        }

        var now = DateTime.UtcNow;
        var order = input.Adapt<Order>();
        order.Status = OrderStatus.Pending;
        order.CreatedAt = now;
        order.UpdatedAt = now;

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return order.Adapt<AddOrderOutDto>();
    }
}

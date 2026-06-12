using Application.DTOs.Customers;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.ExternalServices;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class CustomerSyncService : ICustomerSyncService
{
    private readonly AppDbContext _dbContext;
    private readonly IAraseCustomerClient _customerClient;

    public CustomerSyncService(
        AppDbContext dbContext,
        IAraseCustomerClient customerClient)
    {
        _dbContext = dbContext;
        _customerClient = customerClient;
    }

    public async Task<CustomerSyncOutDto> SyncAsync(CancellationToken cancellationToken)
    {
        var externalCustomers = await _customerClient.GetCustomersAsync(cancellationToken);
        var nationalCodes = externalCustomers
            .Select(customer => customer.NationalCode)
            .Where(nationalCode => !string.IsNullOrWhiteSpace(nationalCode))
            .Distinct()
            .ToArray();

        var existingCustomers = await _dbContext.Customers
            .Where(customer => nationalCodes.Contains(customer.NationalCode))
            .ToDictionaryAsync(customer => customer.NationalCode, cancellationToken);
        var existingCustomerIds = existingCustomers.Values
            .Select(customer => customer.Id)
            .ToArray();
        var customerIdsWithWalletList = await _dbContext.Wallets
            .Where(wallet => existingCustomerIds.Contains(wallet.CustomerId))
            .Select(wallet => wallet.CustomerId)
            .ToListAsync(cancellationToken);
        var customerIdsWithWallet = customerIdsWithWalletList.ToHashSet();

        var now = DateTime.UtcNow;
        var insertedCount = 0;
        var updatedCount = 0;
        var walletCreatedCount = 0;

        foreach (var externalCustomer in externalCustomers.Where(customer => !string.IsNullOrWhiteSpace(customer.NationalCode)))
        {
            if (existingCustomers.TryGetValue(externalCustomer.NationalCode, out var customer))
            {
                UpdateCustomer(customer, externalCustomer, now);
                updatedCount++;

                if (!customerIdsWithWallet.Contains(customer.Id))
                {
                    _dbContext.Wallets.Add(CreateWallet(customer.Id, now));
                    customerIdsWithWallet.Add(customer.Id);
                    walletCreatedCount++;
                }

                continue;
            }

            var newCustomer = CreateCustomer(externalCustomer, now);
            newCustomer.Wallet = CreateWallet(now);

            _dbContext.Customers.Add(newCustomer);
            existingCustomers.Add(newCustomer.NationalCode, newCustomer);
            insertedCount++;
            walletCreatedCount++;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CustomerSyncOutDto
        {
            InsertedCount = insertedCount,
            UpdatedCount = updatedCount,
            WalletCreatedCount = walletCreatedCount,
            Message = "Customer sync completed."
        };
    }

    private static Customer CreateCustomer(AraseCustomerDto source, DateTime now)
    {
        var customer = new Customer
        {
            CreatedAt = now
        };

        UpdateCustomer(customer, source, now);
        return customer;
    }

    private static void UpdateCustomer(Customer customer, AraseCustomerDto source, DateTime now)
    {
        customer.NationalCode = source.NationalCode;
        customer.FirstName = source.FirstName;
        customer.LastName = source.LastName;
        customer.FatherName = source.FatherName;
        customer.BirthCertificationNumber = source.BirthCertificationNumber;
        customer.RegistrationNumber = source.RegistrationNumber;
        customer.BirthDate = source.BirthDate;
        customer.BranchName = source.BranchName;
        customer.MobileNumber = source.MobileNumber;
        customer.UpdatedAt = now;
    }

    private static Wallet CreateWallet(DateTime now)
    {
        return new Wallet
        {
            Balance = 0,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static Wallet CreateWallet(long customerId, DateTime now)
    {
        var wallet = CreateWallet(now);
        wallet.CustomerId = customerId;

        return wallet;
    }
}

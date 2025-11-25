using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using InvestmentFunds.Domain.Entities;
using InvestmentFunds.Domain.Interfaces;
using InvestmentFunds.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace InvestmentFunds.Infrastructure.Repositories;

public class DynamoDBCustomerRepository : ICustomerRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName;

    public DynamoDBCustomerRepository(IAmazonDynamoDB dynamoDb, IOptions<DynamoDBSettings> settings)
    {
        _dynamoDb = dynamoDb;
        _tableName = settings.Value.CustomersTable;
    }

    public async Task<Customer?> GetByIdAsync(string customerId)
    {
        var request = new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "CustomerId", new AttributeValue { S = customerId } }
            }
        };

        var response = await _dynamoDb.GetItemAsync(request);
        return response.Item.Count > 0 ? MapToCustomer(response.Item) : null;
    }

    public async Task<Customer?> GetByEmailAsync(string email)
    {
        var request = new QueryRequest
        {
            TableName = _tableName,
            IndexName = "EmailIndex",
            KeyConditionExpression = "Email = :email",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":email", new AttributeValue { S = email } }
            }
        };

        var response = await _dynamoDb.QueryAsync(request);
        return response.Items.Count > 0 ? MapToCustomer(response.Items[0]) : null;
    }

    public async Task CreateAsync(Customer customer)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            { "CustomerId", new AttributeValue { S = customer.CustomerId } },
            { "Email", new AttributeValue { S = customer.Email } },
            { "Phone", new AttributeValue { S = customer.Phone } },
            { "PasswordHash", new AttributeValue { S = customer.PasswordHash } },
            { "NotificationPreference", new AttributeValue { N = ((int)customer.NotificationPreference).ToString() } },
            { "Balance", new AttributeValue { N = customer.Balance.ToString() } },
            { "CreatedAt", new AttributeValue { S = customer.CreatedAt.ToString("O") } }
        };

        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = item
        };

        await _dynamoDb.PutItemAsync(request);
    }

    public async Task UpdateAsync(Customer customer)
    {
        var request = new UpdateItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "CustomerId", new AttributeValue { S = customer.CustomerId } }
            },
            UpdateExpression = "SET Balance = :balance, NotificationPreference = :pref, UpdatedAt = :updated",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":balance", new AttributeValue { N = customer.Balance.ToString() } },
                { ":pref", new AttributeValue { N = ((int)customer.NotificationPreference).ToString() } },
                { ":updated", new AttributeValue { S = DateTime.UtcNow.ToString("O") } }
            }
        };

        await _dynamoDb.UpdateItemAsync(request);
    }

    public async Task<decimal> GetBalanceAsync(string customerId)
    {
        var customer = await GetByIdAsync(customerId);
        return customer?.Balance ?? 0;
    }

    public async Task UpdateBalanceAsync(string customerId, decimal newBalance)
    {
        var request = new UpdateItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "CustomerId", new AttributeValue { S = customerId } }
            },
            UpdateExpression = "SET Balance = :balance, UpdatedAt = :updated",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":balance", new AttributeValue { N = newBalance.ToString() } },
                { ":updated", new AttributeValue { S = DateTime.UtcNow.ToString("O") } }
            }
        };

        await _dynamoDb.UpdateItemAsync(request);
    }

    private static Customer MapToCustomer(Dictionary<string, AttributeValue> item)
    {
        return new Customer
        {
            CustomerId = item["CustomerId"].S,
            Email = item["Email"].S,
            Phone = item["Phone"].S,
            PasswordHash = item["PasswordHash"].S,
            NotificationPreference = (NotificationPreference)int.Parse(item["NotificationPreference"].N),
            Balance = decimal.Parse(item["Balance"].N),
            CreatedAt = DateTime.Parse(item["CreatedAt"].S)
        };
    }
}
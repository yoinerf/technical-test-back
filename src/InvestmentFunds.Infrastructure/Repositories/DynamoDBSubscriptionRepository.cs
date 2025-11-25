using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using InvestmentFunds.Domain.Entities;
using InvestmentFunds.Domain.Interfaces;
using InvestmentFunds.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace InvestmentFunds.Infrastructure.Repositories;

public class DynamoDBSubscriptionRepository : ISubscriptionRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName;

    public DynamoDBSubscriptionRepository(IAmazonDynamoDB dynamoDb, IOptions<DynamoDBSettings> settings)
    {
        _dynamoDb = dynamoDb;
        _tableName = settings.Value.SubscriptionsTable;
    }

    public async Task<Subscription?> GetByCustomerAndFundAsync(string customerId, string fundId)
    {
        var request = new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "CustomerId", new AttributeValue { S = customerId } },
                { "FundId", new AttributeValue { S = fundId } }
            }
        };

        var response = await _dynamoDb.GetItemAsync(request);
        return response.Item.Count > 0 ? MapToSubscription(response.Item) : null;
    }

    public async Task<List<Subscription>> GetByCustomerIdAsync(string customerId)
    {
        var request = new QueryRequest
        {
            TableName = _tableName,
            KeyConditionExpression = "CustomerId = :customerId",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":customerId", new AttributeValue { S = customerId } }
            }
        };

        var response = await _dynamoDb.QueryAsync(request);
        return response.Items.Select(MapToSubscription).ToList();
    }

    public async Task CreateAsync(Subscription subscription)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            { "CustomerId", new AttributeValue { S = subscription.CustomerId } },
            { "FundId", new AttributeValue { S = subscription.FundId } },
            { "FundName", new AttributeValue { S = subscription.FundName } },
            { "SubscribedAmount", new AttributeValue { N = subscription.SubscribedAmount.ToString() } },
            { "SubscribedAt", new AttributeValue { S = subscription.SubscribedAt.ToString("O") } },
            { "Status", new AttributeValue { S = subscription.Status.ToString() } }
        };

        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = item
        };

        await _dynamoDb.PutItemAsync(request);
    }

    public async Task DeleteAsync(string customerId, string fundId)
    {
        var request = new DeleteItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "CustomerId", new AttributeValue { S = customerId } },
                { "FundId", new AttributeValue { S = fundId } }
            }
        };

        await _dynamoDb.DeleteItemAsync(request);
    }

    private static Subscription MapToSubscription(Dictionary<string, AttributeValue> item)
    {
        return new Subscription
        {
            CustomerId = item["CustomerId"].S,
            FundId = item["FundId"].S,
            FundName = item["FundName"].S,
            SubscribedAmount = decimal.Parse(item["SubscribedAmount"].N),
            SubscribedAt = DateTime.Parse(item["SubscribedAt"].S),
            Status = Enum.Parse<SubscriptionStatus>(item["Status"].S)
        };
    }

    public Task UpdateAsync(Subscription subscription)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            { "CustomerId", new AttributeValue { S = subscription.CustomerId } },
            { "FundId", new AttributeValue { S = subscription.FundId } },
            { "FundName", new AttributeValue { S = subscription.FundName } },
            { "SubscribedAmount", new AttributeValue { N = subscription.SubscribedAmount.ToString() } },
            { "SubscribedAt", new AttributeValue { S = subscription.SubscribedAt.ToString("O") } },
            { "Status", new AttributeValue { S = subscription.Status.ToString() } }
        };

        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = item
        };

        return _dynamoDb.PutItemAsync(request);
    }
}
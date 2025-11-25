using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using InvestmentFunds.Domain.Entities;
using InvestmentFunds.Domain.Interfaces;
using InvestmentFunds.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace InvestmentFunds.Infrastructure.Repositories;

public class DynamoDBTransactionRepository : ITransactionRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName;

    public DynamoDBTransactionRepository(IAmazonDynamoDB dynamoDb, IOptions<DynamoDBSettings> settings)
    {
        _dynamoDb = dynamoDb;
        _tableName = settings.Value.TransactionsTable;
    }

    public async Task<Transaction?> GetByIdAsync(string transactionId)
    {
        var request = new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "TransactionId", new AttributeValue { S = transactionId } }
            }
        };

        var response = await _dynamoDb.GetItemAsync(request);
        return response.Item.Count > 0 ? MapToTransaction(response.Item) : null;
    }

    public async Task<List<Transaction>> GetByCustomerIdAsync(string customerId)
    {
        var request = new QueryRequest
        {
            TableName = _tableName,
            IndexName = "CustomerIndex",
            KeyConditionExpression = "CustomerId = :customerId",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":customerId", new AttributeValue { S = customerId } }
            }
        };

        var response = await _dynamoDb.QueryAsync(request);
        return response.Items.Select(MapToTransaction).ToList();
    }

    public async Task CreateAsync(Transaction transaction)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            { "TransactionId", new AttributeValue { S = transaction.TransactionId } },
            { "CustomerId", new AttributeValue { S = transaction.CustomerId } },
            { "FundId", new AttributeValue { S = transaction.FundId } },
            { "FundName", new AttributeValue { S = transaction.FundName } },
            { "Type", new AttributeValue { S = transaction.Type.ToString() } },
            { "Amount", new AttributeValue { N = transaction.Amount.ToString() } },
            { "Timestamp", new AttributeValue { S = transaction.Timestamp.ToString("O") } },
            { "Status", new AttributeValue { S = transaction.Status.ToString() } },
            { "Description", new AttributeValue { S = transaction.Description } }
        };

        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = item
        };

        await _dynamoDb.PutItemAsync(request);
    }

    private static Transaction MapToTransaction(Dictionary<string, AttributeValue> item)
    {
        return new Transaction
        {
            TransactionId = item["TransactionId"].S,
            CustomerId = item["CustomerId"].S,
            FundId = item["FundId"].S,
            FundName = item["FundName"].S,
            Type = Enum.Parse<TransactionType>(item["Type"].S),
            Amount = decimal.Parse(item["Amount"].N),
            Timestamp = DateTime.Parse(item["Timestamp"].S),
            Status = Enum.Parse<TransactionStatus>(item["Status"].S),
            Description = item["Description"].S
        };
    }
}
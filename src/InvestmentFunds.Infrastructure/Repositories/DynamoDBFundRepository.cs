using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using InvestmentFunds.Domain.Entities;
using InvestmentFunds.Domain.Interfaces;
using InvestmentFunds.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace InvestmentFunds.Infrastructure.Repositories;

public class DynamoDBFundRepository : IFundRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName;

    public DynamoDBFundRepository(IAmazonDynamoDB dynamoDb, IOptions<DynamoDBSettings> settings)
    {
        _dynamoDb = dynamoDb;
        _tableName = settings.Value.FundsTable;
    }

    public async Task<Fund?> GetByIdAsync(string fundId)
    {
        var request = new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "FundId", new AttributeValue { S = fundId } }
            }
        };

        var response = await _dynamoDb.GetItemAsync(request);
        return response.Item.Count > 0 ? MapToFund(response.Item) : null;
    }

    public async Task<List<Fund>> GetAllAsync()
    {
        var request = new ScanRequest
        {
            TableName = _tableName
        };

        var response = await _dynamoDb.ScanAsync(request);
        return response.Items.Select(MapToFund).ToList();
    }

    public async Task CreateAsync(Fund fund)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            { "FundId", new AttributeValue { S = fund.FundId } },
            { "Name", new AttributeValue { S = fund.Name } },
            { "MinAmount", new AttributeValue { N = fund.MinAmount.ToString() } },
            { "Category", new AttributeValue { S = fund.Category } },
            { "IsActive", new AttributeValue { BOOL = fund.IsActive } },
            { "CreatedAt", new AttributeValue { S = fund.CreatedAt.ToString("O") } }
        };

        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = item
        };

        await _dynamoDb.PutItemAsync(request);
    }

    private static Fund MapToFund(Dictionary<string, AttributeValue> item)
    {
        return new Fund
        {
            FundId = item["FundId"].S,
            Name = item["Name"].S,
            MinAmount = decimal.Parse(item["MinAmount"].N),
            Category = item["Category"].S,
            IsActive = item["IsActive"].BOOL,
            CreatedAt = DateTime.Parse(item["CreatedAt"].S)
        };
    }
}
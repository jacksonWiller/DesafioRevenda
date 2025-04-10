using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Shared.Models;

namespace Shared.DataAccess
{
    public class DynamoDbRevendas : RevendasDAO
    {
        private static readonly string REVENDA_TABLE_NAME = "revenda-app-Table-15IYLM6VGQFJT";
        private readonly AmazonDynamoDBClient _dynamoDbClient;

        public DynamoDbRevendas()
        {
            this._dynamoDbClient = new AmazonDynamoDBClient();
        }

        public async Task<Revenda?> GetRevenda(string id)
        {
            var getItemResponse = await this._dynamoDbClient.GetItemAsync(new GetItemRequest(REVENDA_TABLE_NAME,
                new Dictionary<string, AttributeValue>(1)
                {
                    {RevendaMapper.PK, new AttributeValue(id)}
                }));

            return getItemResponse.IsItemSet ? RevendaMapper.RevendaFromDynamoDB(getItemResponse.Item) : null;
        }

        public async Task PutRevenda(Revenda revenda)
        {
            await this._dynamoDbClient.PutItemAsync(REVENDA_TABLE_NAME, RevendaMapper.RevendaToDynamoDb(revenda));
        }

        public async Task DeleteRevenda(string id)
        {
            await this._dynamoDbClient.DeleteItemAsync(REVENDA_TABLE_NAME, new Dictionary<string, AttributeValue>(1)
            {
                {RevendaMapper.PK, new AttributeValue(id)}
            });
        }

        public async Task<RevendaWrapper> GetAllRevendas()
        {
            var data = await this._dynamoDbClient.ScanAsync(new ScanRequest()
            {
                TableName = REVENDA_TABLE_NAME,
                Limit = 20
            });

            var revendas = new List<Revenda>();

            foreach (var item in data.Items)
            {
                revendas.Add(RevendaMapper.RevendaFromDynamoDB(item));
            }

            return new RevendaWrapper(revendas);
        }
    }
}
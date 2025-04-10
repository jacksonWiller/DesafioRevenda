using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace CreateProduct
{
    public class Function
    {

        private static readonly AmazonDynamoDBClient client = new AmazonDynamoDBClient();

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input">The event for the Lambda function handler to process.</param>
        /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
        /// <returns></returns>
        public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest apigProxyEvent,
            ILambdaContext context)
        {

            context.Logger.LogInformation("FunctionHandler invoked.");

            var request = new ListTablesRequest();
            ListTablesResponse response2;

            do
            {
                response2 = await client.ListTablesAsync(request);
                foreach (string name in response2.TableNames)
                    Console.WriteLine(name);
                request.ExclusiveStartTableName = response2.LastEvaluatedTableName;
            } while (response2.LastEvaluatedTableName != null);

            var tableName = "product-service-Table-JP3L8I10S5G6";
            var item = new Dictionary<string, AttributeValue>
            {
                { "id", new AttributeValue { S = "432" } },
                { "Name", new AttributeValue { S = "Product Teste" } },
                { "Price", new AttributeValue { N = "500" } }
            };

            var putItemRequest = new PutItemRequest
            {
                TableName = tableName,
                Item = item
            };

            try
            {
                var response = await client.PutItemAsync(putItemRequest);
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = "Product created successfully",
                    StatusCode = (int)HttpStatusCode.OK,
                };
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"Error creating product: {ex.Message}");
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = "Error creating product",
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                };
            }
        }
    }
}

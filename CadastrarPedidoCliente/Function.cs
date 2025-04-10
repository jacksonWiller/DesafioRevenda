using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Shared.DataAccess;
using Shared.Models;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace CreateRevenda
{
    public class Function
    {
        private readonly RevendasDAO dataAccess;
        public Function()
        {
            dataAccess = new DynamoDbRevendas();
        }

        public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest apigProxyEvent,
            ILambdaContext context)
        {
            context.Logger.LogInformation("FunctionHandler invoked.");

            try
            {
                var revenda = JsonSerializer.Deserialize<Revenda>(apigProxyEvent.Body);

                if (revenda == null)
                {
                    return new APIGatewayHttpApiV2ProxyResponse
                    {
                        Body = "Invalid revenda data",
                        StatusCode = (int)HttpStatusCode.BadRequest,
                    };
                }

                if (revenda.Id == Guid.Empty)
                {
                    revenda.Id = Guid.NewGuid();
                }

                await dataAccess.PutRevenda(revenda);

                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = "Revenda created successfully",
                    StatusCode = (int)HttpStatusCode.Created,
                };
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"Error creating revenda: {ex.Message}");
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = "Error creating revenda",
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                };
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Shared.DataAccess;
using Shared.Models;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace GetRevendas
{
    public class Function
    {
        private readonly RevendasDAO dataAccess;

        public Function()
        {
            dataAccess = new DynamoDbRevendas();
        }

        [LambdaFunction]
        [HttpApi(LambdaHttpMethod.Get, "/revendas")]
        public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogInformation("Obtendo todas as revendas");

            try
            {
                var revendasWrapper = await dataAccess.GetAllRevendas();

                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = JsonSerializer.Serialize(revendasWrapper.Revendas),
                    StatusCode = (int)HttpStatusCode.OK,
                    Headers = new Dictionary<string, string>
                    {
                        { "Content-Type", "application/json" }
                    }
                };
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"Erro ao obter revendas: {ex.Message}");
                context.Logger.LogError(ex.StackTrace);

                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = JsonSerializer.Serialize(new { Error = "Erro ao obter revendas", Message = ex.Message }),
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Headers = new Dictionary<string, string>
                    {
                        { "Content-Type", "application/json" }
                    }
                };
            }
        }
    }
}
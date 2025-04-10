using System;
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

namespace CreateRevenda
{
    // {
    //  "body": "{\"cnpj\":\"12345678901234\",\"razaoSocial\":\"Empresa Exemplo Ltda\",\"nomeFantasia\":\"Exemplo Comercial\",\"email\":\"contato@exemplo.com.br\"}",
    //  "headers": {
    //    "Content-Type": "application/json"
    //  },
    //  "isBase64Encoded": false
    //}

    // {
    //  "body": "{\"cnpj\":\"12345678901234\",\"razaoSocial\":\"Empresa Exemplo Ltda\",\"nomeFantasia\":\"Exemplo Comercial\",\"email\":\"contato@exemplo.com.br\",\"telefones\":[{\"numero\":\"11999998888\"}],\"contatos\":[{\"nome\":\"José Silva\",\"principal\":true}],\"enderecosEntrega\":[{\"logradouro\":\"Avenida Paulista\",\"numero\":\"1000\",\"complemento\":\"Sala 123\",\"bairro\":\"Bela Vista\",\"cidade\":\"São Paulo\",\"estado\":\"SP\",\"cep\":\"01310100\"}]}",
    //  "headers": {
    //    "Content-Type": "application/json"
    //  },
    //  "isBase64Encoded": false
    //}

    public class Function
    {
        private readonly RevendasDAO dataAccess;

        public Function()
        {
            dataAccess = new DynamoDbRevendas();
        }

        [LambdaFunction]
        [HttpApi(LambdaHttpMethod.Post, "/revendas")]
        public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogInformation("FunctionHandler invoked.");

            try
            {
                if (string.IsNullOrWhiteSpace(request.Body))
                {
                    return new APIGatewayHttpApiV2ProxyResponse
                    {
                        Body = "Request body is empty",
                        StatusCode = (int)HttpStatusCode.BadRequest
                    };
                }

                var revenda = JsonSerializer.Deserialize<Revenda>(request.Body);

                if (revenda == null)
                {
                    return new APIGatewayHttpApiV2ProxyResponse
                    {
                        Body = "Invalid revenda data",
                        StatusCode = (int)HttpStatusCode.BadRequest
                    };
                }

                if (revenda.Id == Guid.Empty)
                    revenda.Id = Guid.NewGuid();

                await dataAccess.PutRevenda(revenda);

                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = "Revenda created successfully",
                    StatusCode = (int)HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"Error creating revenda: {ex.Message}");
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = "Error creating revenda",
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
        }
    }
}

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
using Shared.Validators;
using System.Collections.Generic;

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

 /* 
    {
      "body": "{\"cnpj\":\"12345678901234\",\"razaoSocial\":\"Empresa Exemplo Ltda\",\"nomeFantasia\":\"Exemplo Comercial\",\"email\":\"contato@exemplo.com.br\",\"telefones\":[{\"numero\":\"11999998888\"}],\"contatos\":[{\"nome\":\"José Silva\",\"principal\":true}],\"enderecosEntrega\":[{\"logradouro\":\"Avenida Paulista\",\"numero\":\"1000\",\"complemento\":\"Sala 123\",\"bairro\":\"Bela Vista\",\"cidade\":\"São Paulo\",\"estado\":\"SP\",\"cep\":\"01310100\"}]}",
      "headers": {
        "Content-Type": "application/json"
      },
      "isBase64Encoded": false
    }
     
 */



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

                // Inicializa propriedades obrigatórias se não forem fornecidas
                if (revenda.Id == Guid.Empty)
                    revenda.Id = Guid.NewGuid();

                // Garante que os valores padrão estão definidos
                if (revenda.Telefones == null)
                    revenda.Telefones = new List<Telefone>();

                if (revenda.Contatos == null)
                    revenda.Contatos = new List<Contato>();

                if (revenda.EnderecosEntrega == null)
                    revenda.EnderecosEntrega = new List<Endereco>();

                if (revenda.Clientes == null)
                    revenda.Clientes = new List<Cliente>();

                // Define as informações de data e status
                revenda.DataCadastro = DateTime.Now;
                revenda.Ativo = true;


                await dataAccess.PutRevenda(revenda);

                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = JsonSerializer.Serialize(new
                    {
                        Message = "Revenda created successfully",
                        RevendaId = revenda.Id
                    }),
                    StatusCode = (int)HttpStatusCode.Created,
                    Headers = new Dictionary<string, string>
                    {
                        { "Content-Type", "application/json" }
                    }
                };
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"Error creating revenda: {ex.Message}");
                context.Logger.LogError(ex.StackTrace);

                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = JsonSerializer.Serialize(new { Error = "Error creating revenda", Message = ex.Message }),
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
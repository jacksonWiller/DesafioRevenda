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
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

/* 
{
  "body": "{\"revendaId\":\"62b8f422-637f-41bc-a603-36152e50f8af\",\"clienteId\":\"f0070935-b05a-4fbd-aeac-8e87a6db9196\",\"itens\":[{\"produto\":\"Cerveja Artesanal IPA\",\"quantidade\":500,\"precoUnitario\":18.50,\"valorTotal\":555.00},{\"produto\":\"Cerveja Artesanal Pilsen\",\"quantidade\":40,\"precoUnitario\":12.75,\"valorTotal\":510.00},{\"produto\":\"Cerveja Artesanal Stout\",\"quantidade\":15,\"precoUnitario\":22.90,\"valorTotal\":343.50},{\"produto\":\"Cerveja Artesanal Weiss\",\"quantidade\":20,\"precoUnitario\":16.80,\"valorTotal\":336.00}],\"observacoes\":\"Manter refrigerado. Entregar somente no período da manhã.\"}",
  "headers": {
    "Content-Type": "application/json"
  },
  "isBase64Encoded": false
}
 */

namespace CreatePedido
{
    public class PedidoRequest
    {
        [JsonPropertyName("revendaId")]
        public string RevendaId { get; set; } = string.Empty;

        [JsonPropertyName("clienteId")]
        public string ClienteId { get; set; } = string.Empty;

        [JsonPropertyName("itens")]
        public List<ItemPedido> Itens { get; set; } = new List<ItemPedido>();

        [JsonPropertyName("observacoes")]
        public string Observacoes { get; set; } = string.Empty;
    }

    public class Function
    {
        private readonly RevendasDAO dataAccess;

        public Function()
        {
            dataAccess = new DynamoDbRevendas();
        }

        [LambdaFunction]
        [HttpApi(LambdaHttpMethod.Post, "/pedidos")]
        public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogInformation("CreatePedido FunctionHandler invoked.");

            try
            {
                if (string.IsNullOrWhiteSpace(request.Body))
                {
                    return new APIGatewayHttpApiV2ProxyResponse
                    {
                        Body = "O corpo da requisição está vazio",
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                    };
                }

                var pedidoRequest = JsonSerializer.Deserialize<PedidoRequest>(request.Body);

                if (pedidoRequest == null)
                {
                    return new APIGatewayHttpApiV2ProxyResponse
                    {
                        Body = "Dados do pedido inválidos",
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                    };
                }

                if (string.IsNullOrWhiteSpace(pedidoRequest.RevendaId) ||
                    string.IsNullOrWhiteSpace(pedidoRequest.ClienteId) ||
                    pedidoRequest.Itens == null || !pedidoRequest.Itens.Any())
                {
                    return new APIGatewayHttpApiV2ProxyResponse
                    {
                        Body = JsonSerializer.Serialize(new
                        {
                            Error = "Dados incompletos",
                            Message = "RevendaId, ClienteId e pelo menos um item são obrigatórios"
                        }),
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                    };
                }

                var revenda = await dataAccess.GetRevenda(pedidoRequest.RevendaId);

                if (revenda == null)
                {
                    return new APIGatewayHttpApiV2ProxyResponse
                    {
                        Body = JsonSerializer.Serialize(new
                        {
                            Error = "Revenda não encontrada",
                            Message = $"Não foi possível encontrar uma revenda com ID: {pedidoRequest.RevendaId}"
                        }),
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                    };
                }

                var cliente = revenda.Clientes?.FirstOrDefault(c => c.Id.ToString() == pedidoRequest.ClienteId);

                if (cliente == null)
                {
                    return new APIGatewayHttpApiV2ProxyResponse
                    {
                        Body = JsonSerializer.Serialize(new
                        {
                            Error = "Cliente não encontrado",
                            Message = $"Não foi possível encontrar um cliente com ID: {pedidoRequest.ClienteId} nesta revenda"
                        }),
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                    };
                }

                var novoPedido = new PedidoCliente
                {
                    Id = Guid.NewGuid(),
                    ClienteId = cliente.Id.ToString(),
                    Itens = pedidoRequest.Itens,
                    Data = DateTime.Now,
                    Status = StatusPedido.Novo,
                    ValorTotal = pedidoRequest.Itens.Sum(i => i.ValorTotal),
                    Observacoes = pedidoRequest.Observacoes ?? string.Empty
                };

                if (cliente.Pedidos == null)
                {
                    cliente.Pedidos = new List<PedidoCliente>();
                }
                cliente.Pedidos.Add(novoPedido);

                revenda.Clientes?.FirstOrDefault(c => c.Id.ToString() == pedidoRequest.ClienteId).Pedidos.Add(novoPedido);

                await dataAccess.PutRevenda(revenda);

                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = JsonSerializer.Serialize(new
                    {
                        Message = "Pedido criado com sucesso",
                        PedidoId = novoPedido.Id,
                        ClienteId = cliente.Id,
                        RevendaId = revenda.Id
                    }),
                    StatusCode = (int)HttpStatusCode.Created,
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"Erro ao criar pedido: {ex.Message}");
                context.Logger.LogError(ex.StackTrace);

                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = JsonSerializer.Serialize(new
                    {
                        Error = "Erro ao criar pedido",
                        Message = ex.Message
                    }),
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }
        }
    }
}
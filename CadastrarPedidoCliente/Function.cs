using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Shared.Models;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace CadastrarPedidoCliente;

public class Function
{
    private static readonly string TableNamePedidos = "PedidosClientes";
    private static readonly string TableNamePedidosFornecedora = "PedidosFornecedora";
    private static readonly string TableNameRevendas = "Revendas";
    private static readonly string FornecedoraApiUrl = "https://api.Fornecedora.example/pedidos";

    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly HttpClient _httpClient;

    static Function()
    {
        AWSSDKHandler.RegisterXRayForAllServices();
    }

    public Function() : this(new AmazonDynamoDBClient(), new HttpClient())
    {
    }

    public Function(IAmazonDynamoDB dynamoDbClient, HttpClient httpClient)
    {
        _dynamoDbClient = dynamoDbClient;
        _httpClient = httpClient;
    }

    static async Task Main(string[] args)
    {
        using var handlerWrapper = HandlerWrapper.GetHandlerWrapper(
            (APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context) =>
                new Function().FunctionHandler(request, context),
            new SourceGeneratorLambdaJsonSerializer<APIGatewayHttpApiV2ProxyRequest, APIGatewayHttpApiV2ProxyResponse>());

        await LambdaBootstrapBuilder.Create(handlerWrapper.Handler,
                handlerWrapper.Serializer)
            .Build()
            .RunAsync();
    }

    public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        context.Logger.LogInformation("Processando solicitação para cadastro de pedido de cliente");

        try
        {
            // Extrair revendaId do path ou query string
            if (!request.PathParameters.TryGetValue("revendaId", out var revendaIdStr) ||
                !Guid.TryParse(revendaIdStr, out var revendaId))
            {
                return BadRequest("ID da revenda inválido ou não informado");
            }

            // Validar se revenda existe
            var revenda = await ObterRevenda(revendaId);
            if (revenda == null)
            {
                return BadRequest("Revenda não encontrada");
            }

            if (string.IsNullOrEmpty(request.Body))
            {
                return BadRequest("Corpo da requisição está vazio");
            }

            var pedidoClienteRequest = JsonSerializer.Deserialize<PedidoClienteRequest>(request.Body);

            // Validação do pedido
            if (pedidoClienteRequest.ClienteId == Guid.Empty)
            {
                return BadRequest("ID do cliente é obrigatório");
            }

            if (string.IsNullOrWhiteSpace(pedidoClienteRequest.ClienteNome))
            {
                return BadRequest("Nome do cliente é obrigatório");
            }

            if (pedidoClienteRequest.Itens == null || !pedidoClienteRequest.Itens.Any())
            {
                return BadRequest("O pedido deve conter pelo menos um item");
            }

            // Criar pedido do cliente
            var pedidoCliente = new PedidoCliente
            {
                ClienteId = pedidoClienteRequest.ClienteId,
                ClienteNome = pedidoClienteRequest.ClienteNome,
                DataPedido = DateTime.UtcNow
            };

            foreach (var item in pedidoClienteRequest.Itens)
            {
                pedidoCliente.Itens.Add(new ItemPedido
                {
                    CodigoProduto = item.CodigoProduto,
                    NomeProduto = item.NomeProduto,
                    Quantidade = item.Quantidade,
                    PrecoUnitario = item.PrecoUnitario
                });
            }

            // Salvar pedido do cliente
            await SalvarPedidoCliente(pedidoCliente, revendaId);

            // Buscar pedidos pendentes para consolidar um pedido para a Fornecedora
            var pedidosPendentes = await ObterPedidosPendentes(revendaId);
            pedidosPendentes.Add(pedidoCliente);

            // Criar pedido consolidado para a Fornecedora
            var pedidoFornecedora = new PedidoFornecedora
            {
                RevendaId = revendaId,
                RevendaCNPJ = revenda.CNPJ,
                RevendaRazaoSocial = revenda.RazaoSocial
            };

            // Agrupar itens dos pedidos pendentes
            var itensAgrupados = new Dictionary<string, ItemPedido>();

            foreach (var pedido in pedidosPendentes)
            {
                foreach (var item in pedido.Itens)
                {
                    if (itensAgrupados.TryGetValue(item.CodigoProduto, out var itemExistente))
                    {
                        itemExistente.Quantidade += item.Quantidade;
                    }
                    else
                    {
                        itensAgrupados[item.CodigoProduto] = new ItemPedido
                        {
                            CodigoProduto = item.CodigoProduto,
                            NomeProduto = item.NomeProduto,
                            Quantidade = item.Quantidade,
                            PrecoUnitario = item.PrecoUnitario
                        };
                    }
                }
            }

            pedidoFornecedora.Itens.AddRange(itensAgrupados.Values);

            // Verificar se atende ao pedido mínimo
            if (pedidoFornecedora.AtendeMinimoFornecedora())
            {
                // Tenta enviar para a Fornecedora
                bool pedidoEnviado = await EnviarPedidoParaFornecedora(pedidoFornecedora);

                if (pedidoEnviado)
                {
                    // Atualizar status dos pedidos pendentes
                    await AtualizarStatusPedidosPendentes(pedidosPendentes);
                }
                else
                {
                    // Salvar pedido Fornecedora para posterior processamento
                    await SalvarPedidoFornecedora(pedidoFornecedora);
                }
            }
            else
            {
                // Salvar pedido Fornecedora para posterior processamento quando atingir quantidade mínima
                await SalvarPedidoFornecedora(pedidoFornecedora);
            }

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = 201,
                Body = JsonSerializer.Serialize(new
                {
                    Id = pedidoCliente.Id,
                    Itens = pedidoCliente.Itens,
                    Mensagem = "Pedido cadastrado com sucesso"
                }),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Erro ao processar solicitação: {ex}");

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = 500,
                Body = JsonSerializer.Serialize(new { Mensagem = "Erro interno do servidor" }),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
    }

    private APIGatewayHttpApiV2ProxyResponse BadRequest(string mensagem)
    {
        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = 400,
            Body = JsonSerializer.Serialize(new { Mensagem = mensagem }),
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }

    private async Task<Revenda> ObterRevenda(Guid revendaId)
    {
        var table = Table.LoadTable(_dynamoDbClient, TableNameRevendas);
        var document = await table.GetItemAsync(revendaId.ToString());

        if (document == null)
            return null;

        return new Revenda
        {
            Id = Guid.Parse(document["Id"].AsString()),
            CNPJ = document["CNPJ"].AsString(),
            RazaoSocial = document["RazaoSocial"].AsString(),
            NomeFantasia = document["NomeFantasia"].AsString(),
            Email = document["Email"].AsString()
        };
    }

    private async Task SalvarPedidoCliente(PedidoCliente pedido, Guid revendaId)
    {
        var table = Table.LoadTable(_dynamoDbClient, TableNamePedidos);

        var document = new Document
        {
            ["Id"] = pedido.Id.ToString(),
            ["RevendaId"] = revendaId.ToString(),
            ["ClienteId"] = pedido.ClienteId.ToString(),
            ["ClienteNome"] = pedido.ClienteNome,
            ["DataPedido"] = pedido.DataPedido.ToString("o"),
            ["Itens"] = JsonSerializer.Serialize(pedido.Itens),
            ["Status"] = "Pendente"
        };

        await table.PutItemAsync(document);
    }

    private async Task<List<PedidoCliente>> ObterPedidosPendentes(Guid revendaId)
    {
        var table = Table.LoadTable(_dynamoDbClient, TableNamePedidos);
        var scanFilter = new ScanFilter();
        scanFilter.AddCondition("RevendaId", ScanOperator.Equal, revendaId.ToString());
        scanFilter.AddCondition("Status", ScanOperator.Equal, "Pendente");

        var search = table.Scan(scanFilter);
        var documents = await search.GetNextSetAsync();

        var pedidos = new List<PedidoCliente>();

        foreach (var document in documents)
        {
            var pedido = new PedidoCliente
            {
                Id = Guid.Parse(document["Id"].AsString()),
                ClienteId = Guid.Parse(document["ClienteId"].AsString()),
                ClienteNome = document["ClienteNome"].AsString(),
                DataPedido = DateTime.Parse(document["DataPedido"].AsString()),
                Itens = JsonSerializer.Deserialize<List<ItemPedido>>(document["Itens"].AsString())
            };

            pedidos.Add(pedido);
        }

        return pedidos;
    }

    private async Task SalvarPedidoFornecedora(PedidoFornecedora pedido)
    {
        var table = Table.LoadTable(_dynamoDbClient, TableNamePedidosFornecedora);

        var document = new Document
        {
            ["Id"] = pedido.Id.ToString(),
            ["RevendaId"] = pedido.RevendaId.ToString(),
            ["RevendaCNPJ"] = pedido.RevendaCNPJ,
            ["RevendaRazaoSocial"] = pedido.RevendaRazaoSocial,
            ["DataPedido"] = pedido.DataPedido.ToString("o"),
            ["Status"] = pedido.Status.ToString(),
            ["Itens"] = JsonSerializer.Serialize(pedido.Itens),
            ["QuantidadeTotal"] = pedido.QuantidadeTotal
        };

        await table.PutItemAsync(document);
    }

    private async Task AtualizarStatusPedidosPendentes(List<PedidoCliente> pedidos)
    {
        var table = Table.LoadTable(_dynamoDbClient, TableNamePedidos);

        foreach (var pedido in pedidos)
        {
            var document = new Document
            {
                ["Id"] = pedido.Id.ToString(),
                ["Status"] = "Processado"
            };

            await table.UpdateItemAsync(document);
        }
    }

    private async Task<bool> EnviarPedidoParaFornecedora(PedidoFornecedora pedido)
    {
        try
        {
            var pedidoJson = JsonSerializer.Serialize(new
            {
                RevendaId = pedido.RevendaId,
                Cnpj = pedido.RevendaCNPJ,
                RazaoSocial = pedido.RevendaRazaoSocial,
                Itens = pedido.Itens.Select(i => new
                {
                    CodigoProduto = i.CodigoProduto,
                    Quantidade = i.Quantidade
                }).ToList()
            });

            var content = new StringContent(pedidoJson, Encoding.UTF8, "application/json");

            // Adicionar timeout para o caso da API estar instável
            var timeoutCts = new CancellationTokenSource();
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));

            var response = await _httpClient.PostAsync(FornecedoraApiUrl, content, timeoutCts.Token);

            if (response.IsSuccessStatusCode)
            {
                pedido.Status = StatusPedido.Enviado;
                return true;
            }

            pedido.Status = StatusPedido.Erro;
            return false;
        }
        catch (Exception)
        {
            pedido.Status = StatusPedido.Erro;
            return false;
        }
    }

    // Classes auxiliares para receber os dados do request
    public class PedidoClienteRequest
    {
        public Guid ClienteId { get; set; }
        public string ClienteNome { get; set; }
        public List<ItemPedidoRequest> Itens { get; set; }
    }

    public class ItemPedidoRequest
    {
        public string CodigoProduto { get; set; }
        public string NomeProduto { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
    }
}
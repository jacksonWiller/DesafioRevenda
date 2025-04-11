using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Shared.DataAccess;
using Shared.Models;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ProcessarPedidos
{
    public class Function
    {
        private readonly RevendasDAO _dataAccess;
        private readonly HttpClient _httpClient;
        private readonly string _ProdutoraApiUrl = Environment.GetEnvironmentVariable("PRODUTORA_API_URL") ?? "https://api.Produtora.example.com/pedidos";
        private readonly int _pedidoMinimoQuantidade = 1000;

        public Function()
        {
            _dataAccess = new DynamoDbRevendas();
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        [LambdaFunction]
        [HttpApi(LambdaHttpMethod.Post, "/processar")]
        public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogInformation("Iniciando processamento de pedidos de todas as revendas");

            try
            {
                var revendasWrapper = await _dataAccess.GetAllRevendas();
                if (revendasWrapper == null || !revendasWrapper.Revendas.Any())
                {
                    return CriarResposta("Nenhuma revenda encontrada", HttpStatusCode.NotFound);
                }

                var resultados = new List<ResultadoProcessamentoRevenda>();

                foreach (var revenda in revendasWrapper.Revendas)
                {
                    var pedidosPendentes = ObterPedidosPendentes(revenda);

                    if (!pedidosPendentes.Any())
                    {
                        continue;
                    }

                    context.Logger.LogInformation($"Processando revenda {revenda.Id} ({revenda.RazaoSocial}) - {pedidosPendentes.Count} pedidos pendentes");

                    var itensPedidoAgrupados = AgruparItensPedido(pedidosPendentes);
                    int quantidadeTotal = itensPedidoAgrupados.Sum(item => item.Quantidade);

                    var resultadoRevenda = new ResultadoProcessamentoRevenda
                    {
                        RevendaId = revenda.Id.ToString(),
                        RazaoSocial = revenda.RazaoSocial,
                        PedidosPendentes = pedidosPendentes.Count,
                        QuantidadeItens = quantidadeTotal
                    };

                    // Verificar quantidade mínima
                    if (quantidadeTotal < _pedidoMinimoQuantidade)
                    {
                        resultadoRevenda.Status = "Pendente";
                        resultadoRevenda.Mensagem = $"Quantidade total de itens ({quantidadeTotal}) não atingiu o mínimo necessário ({_pedidoMinimoQuantidade})";
                        resultados.Add(resultadoRevenda);
                        continue;
                    }

                    var pedidoProdutora = new PedidoProdutora
                    {
                        RevendaId = revenda.Id.ToString(),
                        RevendaCnpj = revenda.CNPJ,
                        Itens = itensPedidoAgrupados,
                        DataPedido = DateTime.Now
                    };

                    var resultado = await EnviarPedidoParaProdutoraMock(pedidoProdutora, context);
                    await AtualizarStatusPedidos(revenda, pedidosPendentes, resultado.Sucesso);
                    await _dataAccess.PutRevenda(revenda);

                    resultadoRevenda.Status = resultado.Sucesso ? "Processado" : "Erro";
                    resultadoRevenda.Mensagem = resultado.Mensagem;
                    resultadoRevenda.NumeroPedido = resultado.NumeroPedido;
                    resultados.Add(resultadoRevenda);
                }

                if (!resultados.Any())
                {
                    return CriarResposta("Nenhuma revenda com pedidos pendentes encontrada", HttpStatusCode.OK);
                }

                var resumo = new ResumoPedidosProcessados
                {
                    TotalRevendas = resultados.Count,
                    RevendasProcessadas = resultados.Count(r => r.Status == "Processado"),
                    RevendasComErro = resultados.Count(r => r.Status == "Erro"),
                    RevendasPendentes = resultados.Count(r => r.Status == "Pendente"),
                    Detalhes = resultados
                };

                return CriarResposta(resumo, HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"Erro ao processar pedidos: {ex.Message}");
                context.Logger.LogError(ex.StackTrace);
                return CriarResposta($"Erro ao processar pedidos: {ex.Message}", HttpStatusCode.InternalServerError);
            }
        }

        private List<PedidoCliente> ObterPedidosPendentes(Revenda revenda)
        {
            var pedidosPendentes = new List<PedidoCliente>();

            foreach (var cliente in revenda.Clientes)
            {
                var pedidosDoCliente = cliente.Pedidos
                    .Where(p => p.Status == StatusPedido.Novo || p.Status == StatusPedido.Erro)
                    .ToList();

                pedidosPendentes.AddRange(pedidosDoCliente);
            }

            return pedidosPendentes;
        }

        private List<ItemPedidoAgrupado> AgruparItensPedido(List<PedidoCliente> pedidos)
        {
            var itensAgrupados = new Dictionary<string, ItemPedidoAgrupado>();

            foreach (var pedido in pedidos)
            {
                foreach (var item in pedido.Itens)
                {
                    if (!itensAgrupados.ContainsKey(item.Produto))
                    {
                        itensAgrupados[item.Produto] = new ItemPedidoAgrupado
                        {
                            Produto = item.Produto,
                            Quantidade = 0,
                            PrecoUnitario = item.PrecoUnitario,
                            PedidosIds = new List<string>()
                        };
                    }

                    itensAgrupados[item.Produto].Quantidade += item.Quantidade;
                    itensAgrupados[item.Produto].PedidosIds.Add(pedido.Id.ToString());
                }
            }

            return itensAgrupados.Values.ToList();
        }

        private async Task<ResultadoEnvioPedido> EnviarPedidoParaProdutoraMock(PedidoProdutora pedido, ILambdaContext context)
        {
            try
            {
                await Task.Delay(300);

                Random random = new Random();

                bool sucesso = random.Next(10) < 7;

                if (sucesso)
                {
                    // Gerar um número de pedido aleatório
                    string numeroPedido = $"AMB-{random.Next(10000, 99999)}";

                    context.Logger.LogInformation($"[MOCK] Pedido enviado com sucesso para a Produtora. Número: {numeroPedido}");

                    return new ResultadoEnvioPedido
                    {
                        Sucesso = true,
                        NumeroPedido = numeroPedido,
                        Mensagem = "Pedido enviado com sucesso para a Produtora",
                        ItensProcessados = pedido.Itens.Count
                    };
                }
                else
                {
                    string[] errosPossiveis = {
                "Timeout na conexão",
                "Produto indisponível no estoque",
                "Limite de crédito excedido",
                "Problemas no processamento do pedido",
                "Erro no sistema de distribuição"
            };

                    string mensagemErro = errosPossiveis[random.Next(errosPossiveis.Length)];
                    context.Logger.LogWarning($"[MOCK] Falha ao enviar pedido para a Produtora: {mensagemErro}");

                    return new ResultadoEnvioPedido
                    {
                        Sucesso = false,
                        Mensagem = $"Falha ao enviar pedido para a Produtora: {mensagemErro}",
                        ItensProcessados = 0
                    };
                }
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"[MOCK] Exceção ao enviar pedido para a Produtora: {ex.Message}");
                return new ResultadoEnvioPedido
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao conectar com a API da Produtora: {ex.Message}",
                    ItensProcessados = 0
                };
            }
        }

        private async Task<ResultadoEnvioPedido> EnviarPedidoParaProdutoraTrue(PedidoProdutora pedido, ILambdaContext context)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(pedido),
                    System.Text.Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(_ProdutoraApiUrl, content);

                Random random = new Random();
                var responseREST = random.Next(2) == 1;

                if (responseREST)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var pedidoResponse = JsonSerializer.Deserialize<PedidoProdutoraResponse>(responseContent);

                    return new ResultadoEnvioPedido
                    {
                        Sucesso = true,
                        NumeroPedido = pedidoResponse?.NumeroPedido ?? "",
                        Mensagem = "Pedido enviado com sucesso para a Produtora",
                        ItensProcessados = pedido.Itens.Count
                    };
                }
                else
                {
                    context.Logger.LogWarning($"Falha ao enviar pedido para a Produtora: {response.StatusCode}");
                    return new ResultadoEnvioPedido
                    {
                        Sucesso = false,
                        Mensagem = $"Falha ao enviar pedido para a Produtora: {response.StatusCode}",
                        ItensProcessados = 0
                    };
                }
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"Exceção ao enviar pedido para a Produtora: {ex.Message}");
                return new ResultadoEnvioPedido
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao conectar com a API da Produtora: {ex.Message}",
                    ItensProcessados = 0
                };
            }
        }

        private async Task AtualizarStatusPedidos(Revenda revenda, List<PedidoCliente> pedidosPendentes, bool sucesso)
        {
            foreach (var cliente in revenda.Clientes)
            {
                foreach (var pedido in cliente.Pedidos)
                {
                    if (pedidosPendentes.Any(p => p.Id == pedido.Id))
                    {
                        pedido.Status = sucesso ? StatusPedido.Enviado : StatusPedido.Erro;
                    }
                }
            }
        }

        private APIGatewayHttpApiV2ProxyResponse CriarResposta(object body, HttpStatusCode statusCode)
        {
            return new APIGatewayHttpApiV2ProxyResponse
            {
                Body = JsonSerializer.Serialize(body),
                StatusCode = (int)statusCode,
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        }
    }

    public class ProcessarPedidosRequest
    {
        public string RevendaId { get; set; } = "";
    }

    public class ItemPedidoAgrupado
    {
        public string Produto { get; set; } = "";
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public List<string> PedidosIds { get; set; } = new List<string>();
    }

    public class PedidoProdutora
    {
        public string RevendaId { get; set; } = "";
        public string RevendaCnpj { get; set; } = "";
        public List<ItemPedidoAgrupado> Itens { get; set; } = new List<ItemPedidoAgrupado>();
        public DateTime DataPedido { get; set; }
    }

    public class PedidoProdutoraResponse
    {
        public string NumeroPedido { get; set; } = "";
        public List<ItemPedidoAgrupado> Itens { get; set; } = new List<ItemPedidoAgrupado>();
    }

    public class ResultadoEnvioPedido
    {
        public bool Sucesso { get; set; }
        public string NumeroPedido { get; set; } = "";
        public string Mensagem { get; set; } = "";
        public int ItensProcessados { get; set; }
    }

    public class ResultadoProcessamentoRevenda
    {
        public string RevendaId { get; set; } = "";
        public string RazaoSocial { get; set; } = "";
        public int PedidosPendentes { get; set; }
        public int QuantidadeItens { get; set; }
        public string Status { get; set; } = "";
        public string Mensagem { get; set; } = "";
        public string NumeroPedido { get; set; } = "";
    }

    public class ResumoPedidosProcessados
    {
        public int TotalRevendas { get; set; }
        public int RevendasProcessadas { get; set; }
        public int RevendasComErro { get; set; }
        public int RevendasPendentes { get; set; }
        public List<ResultadoProcessamentoRevenda> Detalhes { get; set; } = new List<ResultadoProcessamentoRevenda>();
    }

}
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Shared.Models
{
    public class Endereco
    {
        public Endereco()
        {
            Logradouro = string.Empty;
            Numero = string.Empty;
            Complemento = string.Empty;
            Bairro = string.Empty;
            Cidade = string.Empty;
            Estado = string.Empty;
            CEP = string.Empty;
        }

        public Endereco(string logradouro, string numero, string complemento, string bairro, string cidade, string estado, string cep)
        {
            Logradouro = logradouro;
            Numero = numero;
            Complemento = complemento;
            Bairro = bairro;
            Cidade = cidade;
            Estado = estado;
            CEP = cep;
        }

        [JsonPropertyName("logradouro")]
        public string Logradouro { get; set; }

        [JsonPropertyName("numero")]
        public string Numero { get; set; }

        [JsonPropertyName("complemento")]
        public string Complemento { get; set; }

        [JsonPropertyName("bairro")]
        public string Bairro { get; set; }

        [JsonPropertyName("cidade")]
        public string Cidade { get; set; }

        [JsonPropertyName("estado")]
        public string Estado { get; set; }

        [JsonPropertyName("cep")]
        public string CEP { get; set; }
    }

    public class Telefone
    {
        public Telefone()
        {
            Numero = string.Empty;
        }

        public Telefone(string numero)
        {
            Numero = numero;
        }

        [JsonPropertyName("numero")]
        public string Numero { get; set; }
    }

    public class Contato
    {
        public Contato()
        {
            Nome = string.Empty;
            Principal = false;
            Email = string.Empty;
            Telefone = string.Empty;
        }

        public Contato(string nome, bool principal, string email = "", string telefone = "")
        {
            Nome = nome;
            Principal = principal;
            Email = email;
            Telefone = telefone;
        }

        [JsonPropertyName("nome")]
        public string Nome { get; set; }

        [JsonPropertyName("principal")]
        public bool Principal { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("telefone")]
        public string Telefone { get; set; }
    }

    public class Revenda
    {
        public Revenda()
        {
            Id = Guid.NewGuid();
            CNPJ = string.Empty;
            RazaoSocial = string.Empty;
            NomeFantasia = string.Empty;
            Email = string.Empty;
            Telefones = new List<Telefone>();
            Contatos = new List<Contato>();
            EnderecosEntrega = new List<Endereco>();
            Clientes = new List<Cliente>();
            DataCadastro = DateTime.Now;
            Ativo = true;
        }

        public Revenda(Guid id, string cnpj, string razaoSocial, string nomeFantasia, string email,
            List<Telefone> telefones, List<Contato> contatos, List<Endereco> enderecosEntrega)
        {
            Id = id;
            CNPJ = cnpj;
            RazaoSocial = razaoSocial;
            NomeFantasia = nomeFantasia;
            Email = email;
            Telefones = telefones ?? new List<Telefone>();
            Contatos = contatos ?? new List<Contato>();
            EnderecosEntrega = enderecosEntrega ?? new List<Endereco>();
            Clientes = new List<Cliente>();
            DataCadastro = DateTime.Now;
            Ativo = true;
        }

        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("cnpj")]
        public string CNPJ { get; set; }

        [JsonPropertyName("razaoSocial")]
        public string RazaoSocial { get; set; }

        [JsonPropertyName("nomeFantasia")]
        public string NomeFantasia { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("telefones")]
        public List<Telefone> Telefones { get; set; }

        [JsonPropertyName("contatos")]
        public List<Contato> Contatos { get; set; }

        [JsonPropertyName("enderecosEntrega")]
        public List<Endereco> EnderecosEntrega { get; set; }

        [JsonPropertyName("clientes")]
        public List<Cliente> Clientes { get; set; }

        [JsonPropertyName("dataCadastro")]
        public DateTime DataCadastro { get; set; }

        [JsonPropertyName("ativo")]
        public bool Ativo { get; set; }

        [JsonPropertyName("observacoes")]
        public string Observacoes { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"Revenda{{id='{Id}', cnpj='{CNPJ}', razaoSocial='{RazaoSocial}', " +
                   $"nomeFantasia='{NomeFantasia}', email='{Email}', " +
                   $"telefones='{Telefones.Count}', contatos='{Contatos.Count}', " +
                   $"enderecosEntrega='{EnderecosEntrega.Count}', clientes='{Clientes.Count}', " +
                   $"dataCadastro='{DataCadastro:yyyy-MM-dd}', ativo='{Ativo}'}}";
        }
    }

    public class Cliente
    {
        public Cliente()
        {
            Id = Guid.NewGuid();
            Cnpj = string.Empty;
            RazaoSocial = string.Empty;
            NomeFantasia = string.Empty;
            Email = string.Empty;
            Telefone = string.Empty;
            Endereco = new Endereco();
            Pedidos = new List<PedidoCliente>();
            DataCadastro = DateTime.Now;
            Ativo = true;
        }

        public Cliente(string cnpj, string razaoSocial, string nomeFantasia, string email, string telefone, Endereco endereco)
        {
            Id = Guid.NewGuid();
            Cnpj = cnpj;
            RazaoSocial = razaoSocial;
            NomeFantasia = nomeFantasia;
            Email = email;
            Telefone = telefone;
            Endereco = endereco ?? new Endereco();
            Pedidos = new List<PedidoCliente>();
            DataCadastro = DateTime.Now;
            Ativo = true;
        }

        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("cnpj")]
        public string Cnpj { get; set; }

        [JsonPropertyName("razaoSocial")]
        public string RazaoSocial { get; set; }

        [JsonPropertyName("nomeFantasia")]
        public string NomeFantasia { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("telefone")]
        public string Telefone { get; set; }

        [JsonPropertyName("endereco")]
        public Endereco Endereco { get; set; }

        [JsonPropertyName("pedidos")]
        public List<PedidoCliente> Pedidos { get; set; }

        [JsonPropertyName("dataCadastro")]
        public DateTime DataCadastro { get; set; }

        [JsonPropertyName("ativo")]
        public bool Ativo { get; set; }
    }

    public class PedidoCliente
    {
        public PedidoCliente()
        {
            Id = Guid.NewGuid();
            ClienteId = string.Empty;
            Itens = new List<ItemPedido>();
            Data = DateTime.Now;
            Status = StatusPedido.Novo;
        }

        public PedidoCliente(string clienteId, List<ItemPedido> itens)
        {
            Id = Guid.NewGuid();
            ClienteId = clienteId;
            Itens = itens ?? new List<ItemPedido>();
            Data = DateTime.Now;
            Status = StatusPedido.Novo;
        }

        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("clienteId")]
        public string ClienteId { get; set; }

        [JsonPropertyName("itens")]
        public List<ItemPedido> Itens { get; set; }

        [JsonPropertyName("data")]
        public DateTime Data { get; set; }

        [JsonPropertyName("status")]
        public StatusPedido Status { get; set; }

        [JsonPropertyName("valorTotal")]
        public decimal ValorTotal { get; set; }

        [JsonPropertyName("observacoes")]
        public string Observacoes { get; set; } = string.Empty;
    }

    public enum StatusPedido
    {
        Novo,
        Erro,
        Enviado,
    }

    public class ItemPedido
    {
        public ItemPedido()
        {
            Produto = string.Empty;
            Quantidade = 0;
            PrecoUnitario = 0;
        }

        public ItemPedido(string produto, int quantidade, decimal precoUnitario = 0)
        {
            Produto = produto;
            Quantidade = quantidade;
            PrecoUnitario = precoUnitario;
        }

        [JsonPropertyName("produto")]
        public string Produto { get; set; }

        [JsonPropertyName("quantidade")]
        public int Quantidade { get; set; }

        [JsonPropertyName("precoUnitario")]
        public decimal PrecoUnitario { get; set; }

        [JsonPropertyName("valorTotal")]
        public decimal ValorTotal => Quantidade * PrecoUnitario;
    }
}
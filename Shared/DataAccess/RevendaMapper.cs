using System;
using System.Collections.Generic;
using System.Text.Json;
using Amazon.DynamoDBv2.Model;
using Shared.Models;

namespace Shared.DataAccess
{
    public class RevendaMapper
    {
        public static string PK = "id";
        public static string CNPJ = "cnpj";
        public static string RAZAO_SOCIAL = "razaosocial";
        public static string NOME_FANTASIA = "nomefantasia";
        public static string EMAIL = "email";
        public static string TELEFONES = "telefones";
        public static string CONTATOS = "contatos";
        public static string ENDERECOS = "enderecos";
        public static string CLIENTES = "clientes";
        public static string DATA_CADASTRO = "datacadastro";
        public static string ATIVO = "ativo";
        public static string OBSERVACOES = "observacoes";

        public static Revenda RevendaFromDynamoDB(Dictionary<String, AttributeValue> items)
        {
            var id = Guid.Parse(items[PK].S);
            var telefones = new List<Telefone>();
            var contatos = new List<Contato>();
            var enderecos = new List<Endereco>();
            var clientes = new List<Cliente>();

            // Recuperar telefones
            if (items.ContainsKey(TELEFONES) && !string.IsNullOrEmpty(items[TELEFONES].S))
            {
                telefones = JsonSerializer.Deserialize<List<Telefone>>(items[TELEFONES].S) ?? new List<Telefone>();
            }

            // Recuperar contatos
            if (items.ContainsKey(CONTATOS) && !string.IsNullOrEmpty(items[CONTATOS].S))
            {
                contatos = JsonSerializer.Deserialize<List<Contato>>(items[CONTATOS].S) ?? new List<Contato>();
            }

            // Recuperar endereços
            if (items.ContainsKey(ENDERECOS) && !string.IsNullOrEmpty(items[ENDERECOS].S))
            {
                enderecos = JsonSerializer.Deserialize<List<Endereco>>(items[ENDERECOS].S) ?? new List<Endereco>();
            }

            // Recuperar clientes
            if (items.ContainsKey(CLIENTES) && !string.IsNullOrEmpty(items[CLIENTES].S))
            {
                clientes = JsonSerializer.Deserialize<List<Cliente>>(items[CLIENTES].S) ?? new List<Cliente>();
            }

            // Recuperar data de cadastro
            DateTime dataCadastro = DateTime.Now;
            if (items.ContainsKey(DATA_CADASTRO) && !string.IsNullOrEmpty(items[DATA_CADASTRO].S))
            {
                DateTime.TryParse(items[DATA_CADASTRO].S, out dataCadastro);
            }

            bool ativo = true;
            if (items.ContainsKey(ATIVO) && items[ATIVO].BOOL != null)
            {
                ativo = (bool)items[ATIVO].BOOL;
            }

            // Recuperar observações
            string observacoes = string.Empty;
            if (items.ContainsKey(OBSERVACOES) && !string.IsNullOrEmpty(items[OBSERVACOES].S))
            {
                observacoes = items[OBSERVACOES].S;
            }

            var revenda = new Revenda(
                id,
                items[CNPJ].S,
                items[RAZAO_SOCIAL].S,
                items[NOME_FANTASIA].S,
                items[EMAIL].S,
                telefones,
                contatos,
                enderecos);

            revenda.Clientes = clientes;
            revenda.DataCadastro = dataCadastro;
            revenda.Ativo = ativo;
            revenda.Observacoes = observacoes;

            return revenda;
        }

        public static Dictionary<String, AttributeValue> RevendaToDynamoDb(Revenda revenda)
        {
            Dictionary<String, AttributeValue> item = new Dictionary<string, AttributeValue>();

            item.Add(PK, new AttributeValue(revenda.Id.ToString()));
            item.Add(CNPJ, new AttributeValue(revenda.CNPJ));
            item.Add(RAZAO_SOCIAL, new AttributeValue(revenda.RazaoSocial));
            item.Add(NOME_FANTASIA, new AttributeValue(revenda.NomeFantasia));
            item.Add(EMAIL, new AttributeValue(revenda.Email));

            item.Add(DATA_CADASTRO, new AttributeValue(revenda.DataCadastro.ToString("yyyy-MM-ddTHH:mm:ss")));

            item.Add(ATIVO, new AttributeValue { BOOL = revenda.Ativo });

            if (!string.IsNullOrEmpty(revenda.Observacoes))
            {
                item.Add(OBSERVACOES, new AttributeValue(revenda.Observacoes));
            }

            if (revenda.Telefones != null && revenda.Telefones.Count > 0)
            {
                item.Add(TELEFONES, new AttributeValue(JsonSerializer.Serialize(revenda.Telefones)));
            }

            if (revenda.Contatos != null && revenda.Contatos.Count > 0)
            {
                item.Add(CONTATOS, new AttributeValue(JsonSerializer.Serialize(revenda.Contatos)));
            }

            if (revenda.EnderecosEntrega != null && revenda.EnderecosEntrega.Count > 0)
            {
                item.Add(ENDERECOS, new AttributeValue(JsonSerializer.Serialize(revenda.EnderecosEntrega)));
            }

            if (revenda.Clientes != null && revenda.Clientes.Count > 0)
            {
                item.Add(CLIENTES, new AttributeValue(JsonSerializer.Serialize(revenda.Clientes)));
            }

            return item;
        }
    }
}
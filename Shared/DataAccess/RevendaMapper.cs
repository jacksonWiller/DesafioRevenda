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

        public static Revenda RevendaFromDynamoDB(Dictionary<String, AttributeValue> items)
        {
            var id = Guid.Parse(items[PK].S);
            var telefones = new List<Telefone>();
            var contatos = new List<Contato>();
            var enderecos = new List<Endereco>();

            if (items.ContainsKey(TELEFONES) && !string.IsNullOrEmpty(items[TELEFONES].S))
            {
                telefones = JsonSerializer.Deserialize<List<Telefone>>(items[TELEFONES].S) ?? new List<Telefone>();
            }

            if (items.ContainsKey(CONTATOS) && !string.IsNullOrEmpty(items[CONTATOS].S))
            {
                contatos = JsonSerializer.Deserialize<List<Contato>>(items[CONTATOS].S) ?? new List<Contato>();
            }

            if (items.ContainsKey(ENDERECOS) && !string.IsNullOrEmpty(items[ENDERECOS].S))
            {
                enderecos = JsonSerializer.Deserialize<List<Endereco>>(items[ENDERECOS].S) ?? new List<Endereco>();
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

            return revenda;
        }

        public static Dictionary<String, AttributeValue> RevendaToDynamoDb(Revenda revenda)
        {
            Dictionary<String, AttributeValue> item = new Dictionary<string, AttributeValue>(8);
            item.Add(PK, new AttributeValue(revenda.Id.ToString()));
            item.Add(CNPJ, new AttributeValue(revenda.CNPJ));
            item.Add(RAZAO_SOCIAL, new AttributeValue(revenda.RazaoSocial));
            item.Add(NOME_FANTASIA, new AttributeValue(revenda.NomeFantasia));
            item.Add(EMAIL, new AttributeValue(revenda.Email));

            // Serializar listas para string JSON
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

            return item;
        }
    }
}
using System;
using System.Collections.Generic;
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

        public static Revenda RevendaFromDynamoDB(Dictionary<String, AttributeValue> items)
        {
            var id = Guid.Parse(items[PK].S);
            var revenda = new Revenda(
                id,
                items[CNPJ].S,
                items[RAZAO_SOCIAL].S,
                items[NOME_FANTASIA].S,
                items[EMAIL].S);

            return revenda;
        }

        public static Dictionary<String, AttributeValue> RevendaToDynamoDb(Revenda revenda)
        {
            Dictionary<String, AttributeValue> item = new Dictionary<string, AttributeValue>(5);
            item.Add(PK, new AttributeValue(revenda.Id.ToString()));
            item.Add(CNPJ, new AttributeValue(revenda.CNPJ));
            item.Add(RAZAO_SOCIAL, new AttributeValue(revenda.RazaoSocial));
            item.Add(NOME_FANTASIA, new AttributeValue(revenda.NomeFantasia));
            item.Add(EMAIL, new AttributeValue(revenda.Email));

            return item;
        }
    }
}
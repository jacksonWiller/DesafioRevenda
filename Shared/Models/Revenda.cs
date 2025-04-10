using System;
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
        }

        public Contato(string nome, bool principal)
        {
            Nome = nome;
            Principal = principal;
        }

        [JsonPropertyName("nome")]
        public string Nome { get; set; }

        [JsonPropertyName("principal")]
        public bool Principal { get; set; }
    }

    public class Revenda
    {
        public Revenda()
        {
            this.Id = Guid.NewGuid();
            this.CNPJ = string.Empty;
            this.RazaoSocial = string.Empty;
            this.NomeFantasia = string.Empty;
            this.Email = string.Empty;
            this.Telefones = new List<Telefone>();
            this.Contatos = new List<Contato>();
            this.EnderecosEntrega = new List<Endereco>();
        }

        public Revenda(Guid id, string cnpj, string razaoSocial, string nomeFantasia, string email,
            List<Telefone> telefones, List<Contato> contatos, List<Endereco> enderecosEntrega)
        {
            this.Id = id;
            this.CNPJ = cnpj;
            this.RazaoSocial = razaoSocial;
            this.NomeFantasia = nomeFantasia;
            this.Email = email;
            this.Telefones = telefones ?? new List<Telefone>();
            this.Contatos = contatos ?? new List<Contato>();
            this.EnderecosEntrega = enderecosEntrega ?? new List<Endereco>();
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

        public override string ToString()
        {
            return "Revenda{" +
                   "id='" + this.Id + '\'' +
                   ", cnpj='" + this.CNPJ + '\'' +
                   ", razaoSocial='" + this.RazaoSocial + '\'' +
                   ", nomeFantasia='" + this.NomeFantasia + '\'' +
                   ", email='" + this.Email + '\'' +
                   ", telefones='" + this.Telefones.Count + '\'' +
                   ", contatos='" + this.Contatos.Count + '\'' +
                   ", enderecosEntrega='" + this.EnderecosEntrega.Count + '\'' +
                   '}';
        }
    }
}
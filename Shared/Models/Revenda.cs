using System;

namespace Shared.Models
{
    public class Revenda
    {
        public Guid Id { get; set; }
        public string CNPJ { get; set; }
        public string RazaoSocial { get; set; }
        public string NomeFantasia { get; set; }
        public string Email { get; set; }

        public ValueObjects Telefone { get; set; }
        public Contato Contato { get; set; }
        public Endereco Endereco { get; set; }

        public Revenda(
            string cnpj,
            string razaoSocial,
            string nomeFantasia,
            string email,
            string telefoneNumero,
            string contatoNome,
            string logradouro,
            string numero,
            string complemento,
            string bairro,
            string cidade,
            string estado,
            string cep
            )
        {
            Id = Guid.NewGuid();
            CNPJ = cnpj;
            RazaoSocial = razaoSocial;
            NomeFantasia = nomeFantasia;
            Email = email;

            Telefone = new ValueObjects
            {
                Id = Guid.NewGuid(),
                Numero = telefoneNumero
            };

            Contato = new Contato
            {
                Id = Guid.NewGuid(),
                Nome = contatoNome,
                Principal = true
            };

            Endereco = new Endereco
            {
                Id = Guid.NewGuid(),
                Logradouro = logradouro,
                Numero = numero,
                Complemento = complemento,
                Bairro = bairro,
                Cidade = cidade,
                Estado = estado,
                CEP = cep
            };
        }
    }
}
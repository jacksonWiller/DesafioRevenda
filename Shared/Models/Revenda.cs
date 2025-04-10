using System;

namespace Shared.Models
{
    public class Revenda
    {
        public Revenda()
        {
            this.Id = Guid.NewGuid();
            this.CNPJ = string.Empty;
            this.RazaoSocial = string.Empty;
            this.NomeFantasia = string.Empty;
            this.Email = string.Empty;
        }

        public Revenda(Guid id, string cnpj, string razaoSocial, string nomeFantasia, string email)
        {
            this.Id = id;
            this.CNPJ = cnpj;
            this.RazaoSocial = razaoSocial;
            this.NomeFantasia = nomeFantasia;
            this.Email = email;
        }

        public Guid Id { get; set; }
        public string CNPJ { get; set; }
        public string RazaoSocial { get; set; }
        public string NomeFantasia { get; set; }
        public string Email { get; set; }

        public override string ToString()
        {
            return "Revenda{" +
                   "id='" + this.Id + '\'' +
                   ", cnpj='" + this.CNPJ + '\'' +
                   ", razaoSocial='" + this.RazaoSocial + '\'' +
                   ", nomeFantasia='" + this.NomeFantasia + '\'' +
                   ", email='" + this.Email + '\'' +
                   '}';
        }
    }
}
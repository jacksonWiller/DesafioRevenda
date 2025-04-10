using System;
using System.Linq;
using System.Text.RegularExpressions;
using Shared.Models;

namespace Shared.Validators
{
    public class RevendaValidator
    {
        public static ValidationResult Validate(Revenda revenda)
        {
            var result = new ValidationResult();

            // CNPJ validation
            if (string.IsNullOrWhiteSpace(revenda.CNPJ))
            {
                result.AddError("CNPJ é obrigatório");
            }
            else if (!IsCnpjValid(revenda.CNPJ))
            {
                result.AddError("CNPJ inválido");
            }

            // Razão Social validation
            if (string.IsNullOrWhiteSpace(revenda.RazaoSocial))
            {
                result.AddError("Razão Social é obrigatória");
            }

            // Nome Fantasia validation
            if (string.IsNullOrWhiteSpace(revenda.NomeFantasia))
            {
                result.AddError("Nome Fantasia é obrigatório");
            }

            // Email validation
            if (string.IsNullOrWhiteSpace(revenda.Email))
            {
                result.AddError("Email é obrigatório");
            }
            else if (!IsEmailValid(revenda.Email))
            {
                result.AddError("Email inválido");
            }

            // Telefones validation (optional)
            if (revenda.Telefones != null)
            {
                foreach (var telefone in revenda.Telefones)
                {
                    if (!string.IsNullOrWhiteSpace(telefone.Numero) && !IsTelefoneValid(telefone.Numero))
                    {
                        result.AddError("Número de telefone inválido");
                    }
                }
            }

            // Contatos validation
            if (revenda.Contatos == null || !revenda.Contatos.Any())
            {
                result.AddError("Pelo menos um contato é obrigatório");
            }
            else if (!revenda.Contatos.Any(c => c.Principal))
            {
                result.AddError("Deve haver pelo menos um contato principal");
            }

            // Endereços validation
            if (revenda.EnderecosEntrega == null || !revenda.EnderecosEntrega.Any())
            {
                result.AddError("Pelo menos um endereço de entrega é obrigatório");
            }
            else
            {
                foreach (var endereco in revenda.EnderecosEntrega)
                {
                    if (string.IsNullOrWhiteSpace(endereco.Logradouro) ||
                        string.IsNullOrWhiteSpace(endereco.Numero) ||
                        string.IsNullOrWhiteSpace(endereco.Bairro) ||
                        string.IsNullOrWhiteSpace(endereco.Cidade) ||
                        string.IsNullOrWhiteSpace(endereco.Estado) ||
                        string.IsNullOrWhiteSpace(endereco.CEP))
                    {
                        result.AddError("Campos obrigatórios do endereço não preenchidos");
                    }
                }
            }

            return result;
        }

        private static bool IsCnpjValid(string cnpj)
        {
            // Remove caracteres não numéricos
            cnpj = Regex.Replace(cnpj, "[^0-9]", "");

            // CNPJ deve ter 14 dígitos
            if (cnpj.Length != 14)
                return false;

            // Verifica se todos os dígitos são iguais (caso inválido)
            if (new string(cnpj[0], 14) == cnpj)
                return false;

            // Implementação da validação do algoritmo do CNPJ
            int[] multiplicador1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCnpj = cnpj.Substring(0, 12);
            int soma = 0;

            for (int i = 0; i < 12; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];

            int resto = (soma % 11);
            resto = resto < 2 ? 0 : 11 - resto;

            string digito = resto.ToString();
            tempCnpj += digito;
            soma = 0;

            for (int i = 0; i < 13; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];

            resto = (soma % 11);
            resto = resto < 2 ? 0 : 11 - resto;

            digito += resto.ToString();

            return cnpj.EndsWith(digito);
        }

        private static bool IsEmailValid(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsTelefoneValid(string telefone)
        {
            // Remove caracteres não numéricos
            telefone = Regex.Replace(telefone, "[^0-9]", "");

            // Telefone deve ter entre 10 e 11 dígitos
            return telefone.Length >= 10 && telefone.Length <= 11;
        }
    }

    public class ValidationResult
    {
        public bool IsValid => !Errors.Any();
        public System.Collections.Generic.List<string> Errors { get; } = new System.Collections.Generic.List<string>();

        public void AddError(string error)
        {
            Errors.Add(error);
        }

        public string GetErrorsAsString()
        {
            return string.Join(", ", Errors);
        }
    }
}
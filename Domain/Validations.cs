using FluentValidation;
using System.Text.RegularExpressions;

namespace DESAFIO2.Domain;

public class ClienteCadastroValidator : AbstractValidator<ClienteCadastro>
{
  public ClienteCadastroValidator()
  {
    RuleFor(x => x.Basicos.Nome).NotEmpty().MinimumLength(3);
    RuleFor(x => x.Basicos.Email).NotEmpty().EmailAddress();
    RuleFor(x => x.Basicos.Telefone).NotEmpty().Must(TelefoneValido).WithMessage("Telefone inválido");
    RuleFor(x => x.Basicos.Cpf).NotEmpty().Must(CpfValido).WithMessage("CPF inválido");
    RuleFor(x => x.Basicos.DataNascimento).Must(MaiorIdade).WithMessage("Cliente deve ser maior de 18 anos");
    RuleFor(x => x.Financeiros).Must(f => (f.Renda + f.Patrimonio) > 1000m)
      .WithMessage("Renda + Patrimônio deve ser > 1000,00");
    RuleFor(x => x.Endereco.Cep).NotEmpty().Matches(@"^\d{8}$").WithMessage("CEP deve ter 8 dígitos");
    RuleFor(x => x.Seguranca.Senha).NotEmpty().MinimumLength(6);
    RuleFor(x => x.Seguranca.ConfirmacaoSenha)
      .Equal(x => x.Seguranca.Senha).WithMessage("Confirmação de senha diferente");
  }

  private static bool MaiorIdade(DateOnly data) {
    var hoje = DateOnly.FromDateTime(DateTime.Today);
    var idade = hoje.Year - data.Year - (new DateOnly(hoje.Year, data.Month, data.Day) > hoje ? 1 : 0);
    return idade >= 18;
  }

  private static bool TelefoneValido(string tel) =>
    Regex.IsMatch(tel ?? "", @"^\+?\d{10,15}$"); // simples e suficiente p/ desafio

  // validador compacto de CPF
  private static bool CpfValido(string cpf)
  {
    if (string.IsNullOrWhiteSpace(cpf)) return false;
    var digits = new string(cpf.Where(char.IsDigit).ToArray());
    if (digits.Length != 11) return false;
    if (new string(digits[0], 11) == digits) return false;

    int Calc(string s, int len)
    {
      int sum = 0, weight = len + 1;
      for (int i = 0; i < len; i++) sum += (s[i] - '0') * (weight--);
      int mod = sum % 11; var dv = mod < 2 ? 0 : 11 - mod;
      return dv;
    }

    var dv1 = Calc(digits, 9);
    var dv2 = Calc(digits, 10);
    return digits[9] - '0' == dv1 && digits[10] - '0' == dv2;
  }
}

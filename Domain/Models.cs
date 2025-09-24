namespace DESAFIO2.Domain;

public record DadosBasicos(string Nome, DateOnly DataNascimento, string Cpf, string Email, string Telefone);
public record DadosFinanceiros(decimal Renda, decimal Patrimonio);
public record Endereco(string Rua, string Numero, string Bairro, string Cidade, string Estado, string Cep);
public record DadosSeguranca(string Senha, string ConfirmacaoSenha);

public class ClienteCadastro {
  public DadosBasicos Basicos { get; init; } = default!;
  public DadosFinanceiros Financeiros { get; init; } = default!;
  public Endereco Endereco { get; init; } = default!;
  public DadosSeguranca Seguranca { get; init; } = default!;
}

public record EmailFilaMsg(string Nome, string Email);

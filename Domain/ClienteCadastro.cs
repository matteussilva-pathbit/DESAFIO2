namespace DESAFIO2.Domain;

public record ClienteCadastro(
    DadosBasicos Basicos,
    DadosFinanceiros Financeiros,
    Endereco Endereco,
    DadosSeguranca Seguranca
);

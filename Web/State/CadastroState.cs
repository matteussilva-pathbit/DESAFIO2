using DESAFIO2.Domain;

namespace DESAFIO2.Web.State;

public class CadastroState
{
    public DadosBasicos? Basicos { get; set; }
    public DadosFinanceiros? Financeiros { get; set; }
    public Endereco? Endereco { get; set; }
    public DadosSeguranca? Seguranca { get; set; }
}

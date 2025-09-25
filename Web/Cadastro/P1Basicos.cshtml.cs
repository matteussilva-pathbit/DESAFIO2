using DESAFIO2.Web.State;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DESAFIO2.Web.Cadastro;

public class P1BasicosModel : PageModel
{
    private readonly CadastroState _state;
    public P1BasicosModel(CadastroState state) => _state = state;

    [BindProperty] public string Nome { get; set; } = "";
    [BindProperty] public DateTime DataNascimento { get; set; }   // usa DateTime e converte pra DateOnly
    [BindProperty] public string Cpf { get; set; } = "";
    [BindProperty] public string Email { get; set; } = "";
    [BindProperty] public string Telefone { get; set; } = "";

    public void OnGet()
    {
        if (_state.Basicos is null) return;

        Nome = _state.Basicos.Nome;
        // DateOnly -> DateTime (para reexibir no input type="date")
        DataNascimento = new DateTime(_state.Basicos.DataNascimento.Year,
                                      _state.Basicos.DataNascimento.Month,
                                      _state.Basicos.DataNascimento.Day);
        Cpf = _state.Basicos.Cpf;
        Email = _state.Basicos.Email;
        Telefone = _state.Basicos.Telefone;
    }

    public IActionResult OnPost()
    {
        _state.Basicos = new DESAFIO2.Domain.DadosBasicos(
            Nome,
            DateOnly.FromDateTime(DataNascimento),
            Cpf,
            Email,
            Telefone
        );

        return RedirectToPage("/Cadastro/P2Financeiros");
    }
}

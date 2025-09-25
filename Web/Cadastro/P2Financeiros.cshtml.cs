using DESAFIO2.Domain;
using DESAFIO2.Web.State;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DESAFIO2.Web.Cadastro;

public class P2FinanceirosModel : PageModel
{
    private readonly CadastroState _state;
    public P2FinanceirosModel(CadastroState state) => _state = state;

    [BindProperty] public decimal Renda { get; set; }
    [BindProperty] public decimal Patrimonio { get; set; }

    public IActionResult OnGet()
    {
        if (_state.Basicos is null) return RedirectToPage("/Cadastro/P1Basicos");

        if (_state.Financeiros is not null)
        {
            Renda = _state.Financeiros.Renda;
            Patrimonio = _state.Financeiros.Patrimonio;
        }

        return Page();
    }

    public IActionResult OnPost()
    {
        if (_state.Basicos is null) return RedirectToPage("/Cadastro/P1Basicos");

        _state.Financeiros = new DadosFinanceiros(Renda, Patrimonio);
        return RedirectToPage("/Cadastro/P3Endereco");
    }
}

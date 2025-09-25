using DESAFIO2.Domain;
using DESAFIO2.Web.State;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DESAFIO2.Web.Cadastro;

public class P3EnderecoModel : PageModel
{
    private readonly CadastroState _state;
    public P3EnderecoModel(CadastroState state) => _state = state;

    [BindProperty] public string Cep { get; set; } = "";
    [BindProperty] public string Rua { get; set; } = "";
    [BindProperty] public string Numero { get; set; } = "";
    [BindProperty] public string Bairro { get; set; } = "";
    [BindProperty] public string Cidade { get; set; } = "";
    [BindProperty] public string Estado { get; set; } = "";

    public IActionResult OnGet()
    {
        if (_state.Financeiros is null) return RedirectToPage("/Cadastro/P2Financeiros");

        if (_state.Endereco is not null)
        {
            Cep = _state.Endereco.Cep;
            Rua = _state.Endereco.Rua;
            Numero = _state.Endereco.Numero;
            Bairro = _state.Endereco.Bairro;
            Cidade = _state.Endereco.Cidade;
            Estado = _state.Endereco.Estado;
        }

        return Page();
    }

    public IActionResult OnPost()
    {
        if (_state.Financeiros is null) return RedirectToPage("/Cadastro/P2Financeiros");

        _state.Endereco = new Endereco(Rua, Numero, Bairro, Cidade, Estado, Cep);
        return RedirectToPage("/Cadastro/P4Seguranca");
    }
}

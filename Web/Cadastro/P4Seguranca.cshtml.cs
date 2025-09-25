using DESAFIO2.Domain;
using DESAFIO2.Web.Services;
using DESAFIO2.Web.State;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DESAFIO2.Web.Cadastro;

public class P4SegurancaModel : PageModel
{
    private readonly CadastroState _state;
    private readonly WebApiClient _api;

    public P4SegurancaModel(CadastroState state, WebApiClient api)
    {
        _state = state;
        _api = api;
    }

    [BindProperty] public string Senha { get; set; } = "";
    [BindProperty] public string ConfirmacaoSenha { get; set; } = "";
    public string Erro { get; set; } = "";

    public IActionResult OnGet()
    {
        if (_state.Endereco is null) return RedirectToPage("/Cadastro/P3Endereco");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (_state.Basicos is null || _state.Financeiros is null || _state.Endereco is null)
            return RedirectToPage("/Cadastro/P1Basicos");

        _state.Seguranca = new DadosSeguranca(Senha, ConfirmacaoSenha);

        var cad = new ClienteCadastro(
            _state.Basicos!,
            _state.Financeiros!,
            _state.Endereco!,
            _state.Seguranca!
        );

        var result = await _api.EnviarCadastroAsync(cad);
        if (!result.Success)
        {
            Erro = result.Error ?? "Falha ao enviar cadastro. Tente novamente.";
            return Page();
        }

        // zera o estado se quiser evitar reenvio com F5
        //_state.Basicos = null; _state.Financeiros = null; _state.Endereco = null; _state.Seguranca = null;

        return RedirectToPage("/Cadastro/Concluido");
    }
}

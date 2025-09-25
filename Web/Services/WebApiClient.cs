using DESAFIO2.Domain;
using System.Net.Http.Json;

namespace DESAFIO2.Web.Services;

public record ApiResult(bool Success, string? Error);

public class WebApiClient
{
    private readonly HttpClient _http;
    public WebApiClient(HttpClient http) { _http = http; }

    public async Task<ApiResult> EnviarCadastroAsync(ClienteCadastro cad)
    {
        var resp = await _http.PostAsJsonAsync("http://localhost:5157/api/cadastros", cad);
        if (resp.IsSuccessStatusCode) return new ApiResult(true, null);

        string? body = null;
        try { body = await resp.Content.ReadAsStringAsync(); } catch { }

        var msg = !string.IsNullOrWhiteSpace(body)
            ? body
            : $"Falha HTTP {(int)resp.StatusCode} ({resp.StatusCode})";

        return new ApiResult(false, msg);
    }

    public async Task<Endereco?> BuscarCepAsync(string cep)
        => await _http.GetFromJsonAsync<Endereco?>($"http://localhost:5157/api/cep/{cep}");
}

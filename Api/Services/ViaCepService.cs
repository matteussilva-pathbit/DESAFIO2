using DESAFIO2.Domain;
using System.Net.Http.Json;

namespace DESAFIO2.Api.Services;

public class ViaCepService
{
  private readonly HttpClient _http;
  public ViaCepService(HttpClient http) => _http = http;

  public async Task<Endereco?> BuscarAsync(string cep)
  {
    var resp = await _http.GetFromJsonAsync<ViaCepDto>($"https://viacep.com.br/ws/{cep}/json/");
    if (resp is null || resp.erro == true) return null;
    return new Endereco(resp.logradouro, "", resp.bairro, resp.localidade, resp.uf, cep);
  }

  private record ViaCepDto(string logradouro, string bairro, string localidade, string uf, bool? erro);
}

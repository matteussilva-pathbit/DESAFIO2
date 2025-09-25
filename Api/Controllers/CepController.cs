using DESAFIO2.Api.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class CepController : ControllerBase
{
    private readonly ViaCepService _viaCep;
    public CepController(ViaCepService viaCep) => _viaCep = viaCep;

    [HttpGet("{cep}")]
    public async Task<IActionResult> Get(string cep)
    {
        var end = await _viaCep.BuscarAsync(cep);
        return end is null ? NotFound(new { message = "CEP não encontrado" }) : Ok(end);
    }
}

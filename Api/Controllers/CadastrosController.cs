using DESAFIO2.Domain;
using DESAFIO2.Api.Services;
using DESAFIO2.Api.Infra;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using BCrypt.Net;
using System.Text.Json;

namespace DESAFIO2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CadastrosController : ControllerBase
{
  private readonly IValidator<ClienteCadastro> _validator;
  private readonly ViaCepService _cep;
  private readonly IMongoCollection<ClienteCadastroPersistido> _col;
  private readonly RabbitService _rabbit;

  public CadastrosController(
    IValidator<ClienteCadastro> validator,
    ViaCepService cep,
    IOptions<MongoSettings> mongoOpt,
    MongoContext mongoCtx,
    RabbitService rabbit)
  {
    _validator = validator;
    _cep = cep;
    _col = mongoCtx.GetCollection<ClienteCadastroPersistido>(mongoOpt);
    _rabbit = rabbit;
  }

  [HttpPost]
  public async Task<IActionResult> Post([FromBody] ClienteCadastro model)
  {
    var v = await _validator.ValidateAsync(model);
    if (!v.IsValid) return BadRequest(v.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

    // completa endereço via CEP (se vier apenas CEP/básicos)
    var viaCep = await _cep.BuscarAsync(model.Endereco.Cep);
    var endereco = viaCep is null ? model.Endereco : model.Endereco with {
      Rua = string.IsNullOrWhiteSpace(model.Endereco.Rua) ? viaCep.Rua : model.Endereco.Rua,
      Bairro = string.IsNullOrWhiteSpace(model.Endereco.Bairro) ? viaCep.Bairro : model.Endereco.Bairro,
      Cidade = string.IsNullOrWhiteSpace(model.Endereco.Cidade) ? viaCep.Cidade : model.Endereco.Cidade,
      Estado = string.IsNullOrWhiteSpace(model.Endereco.Estado) ? viaCep.Estado : model.Endereco.Estado
    };

    var hash = BCrypt.Net.BCrypt.HashPassword(model.Seguranca.Senha);
    var persist = new ClienteCadastroPersistido(model.Basicos, model.Financeiros, endereco, PasswordHash: hash, CriadoEm: DateTime.UtcNow);
    await _col.InsertOneAsync(persist);

    var msg = JsonSerializer.Serialize(new EmailFilaMsg(model.Basicos.Nome, model.Basicos.Email));
    await _rabbit.PublishJsonAsync(msg);

    return Accepted(new { status = "em_analise" });
  }

  // DTO de persistência (com hash e metadados)
  public record ClienteCadastroPersistido(DadosBasicos Basicos, DadosFinanceiros Financeiros, Endereco Endereco, string PasswordHash, DateTime CriadoEm);
}

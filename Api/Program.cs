using DESAFIO2.Domain;
using DESAFIO2.Api.Services;
using DESAFIO2.Api.Infra;
using FluentValidation;


var builder = WebApplication.CreateBuilder(args);

// Configurações
builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("Mongo"));
builder.Services.Configure<RabbitSettings>(builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddSingleton<MongoContext>();
builder.Services.AddSingleton<RabbitService>();
builder.Services.AddHttpClient<ViaCepService>();
builder.Services.AddScoped<IValidator<ClienteCadastro>, ClienteCadastroValidator>();


// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();

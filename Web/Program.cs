using DESAFIO2.Web.Services;
using DESAFIO2.Web.State;

var builder = WebApplication.CreateBuilder(args);


builder.Services
    .AddRazorPages()
    .AddRazorPagesOptions(o =>
    {
        // <- muda a raiz padrão de /Pages para /Cadastro
        o.RootDirectory = "/Cadastro";
    });

builder.Services.AddHttpClient<WebApiClient>();
builder.Services.AddScoped<CadastroState>();
builder.Services.AddCors(p => p.AddDefaultPolicy(b => b
.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));



var app = builder.Build();
app.UseCors();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();

// Mapeia Razor Pages
app.MapRazorPages();

app.Run();

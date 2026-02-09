using CashFlow.Api.Filters;
using CashFlow.Api.Token;
using CashFlow.API.Middleware;
using CashFlow.Application;
using CashFlow.Domain.Security.Tokens;
using CashFlow.Infrastructure;
using CashFlow.Infrastructure.Extensions;
using CashFlow.Infrastructure.Migrations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text; // <--- Adicione este using para o Scalar

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi(); // Gera o JSON de especificação OpenAPI

builder.Services.AddMvc(options => options.Filters.Add(typeof(ExceptionFilter))); // adicionando o filtro global de exceções

builder.Services.AddInfrastructure(builder.Configuration); // Adiciona as injeções de dependência da infraestrutura
builder.Services.AddApplication(); // Adiciona as injeções de dependência da aplicação
builder.Services.AddScoped<ITokenProvider, HttpContextTokenValue>(); // Adiciona a implementação de ITokenProvider que obtém o token do contexto HTTP, necessário para obter o token nas requisições e usar em outras classes, como no LoggedUser
builder.Services.AddHttpClient(); // Adiciona o HttpClient para ser injetado em outras classes, necessário para o HttpContextTokenValue

var signingKey = builder.Configuration.GetValue<string>("Settings:Jwt:SigningKey"); // Obtém a chave de assinatura do JWT do arquivo de configuração, GetValue vem do nuget Microsoft.Extensions.Configuration.Binder

/*
 * AddAuthentication configura a autenticação usando JWT Bearer tokens.
 * Precisou instalar o pacote NuGet Microsoft.AspNetCore.Authentication.JwtBearer
 */
builder.Services.AddAuthentication(config =>
{
    config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(config =>
{
    config.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = new TimeSpan(0),
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey!)) // Usa a chave de assinatura para validar o token, mesma linha de racionício do GenerateToken na geração do token
    };
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Cria a rota /openapi/v1.json
    app.MapScalarApiReference(); // <--- Cria a tela visual em /scalar/v1
}
app.UseMiddleware<CultureMiddleware>(); // Adiciona o middleware de cultura, serve para definir a cultura com base no cabeçalho "Accept-Language"
app.UseHttpsRedirection();
/*
 * UseAuthentication habilita o middleware de autenticação, que verifica os tokens JWT nas requisições.
 * UseAuthorization habilita o middleware de autorização, que protege os endpoints com base nas políticas definidas.
 */
app.UseAuthentication();
app.UseAuthorization();

// Mapeia os controllers para as rotas
app.MapControllers();

// Verifica se a aplicação está rodando em um ambiente de teste, se não estiver, executa as migrações do banco de dados 
if (builder.Configuration.IsTestEnvironment() == false)
{
    // Executa as migrations do banco de dados ao iniciar a aplicação
    await MigrateDatabase();
}


app.Run();

async Task MigrateDatabase()
{
    // Cria um escopo para obter os serviços necessários
    await using var scope = app.Services.CreateAsyncScope();

    // Chama o método de migration do banco de dados. e como se fosse uma injeção de dependência
    await DatabaseMigration.MigrateDatabase(scope.ServiceProvider);
}

public partial class Program { } // Necessário para os testes de integração, permite que o WebApplicationFactory acesse a classe Program
using CashFlow.Api.Filters;
using CashFlow.API.Middleware;
using CashFlow.Application;
using CashFlow.Infrastructure;
using CashFlow.Infrastructure.Migrations;
using Scalar.AspNetCore; // <--- Adicione este using para o Scalar

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi(); // Gera o JSON

builder.Services.AddMvc(options => options.Filters.Add(typeof(ExceptionFilter)));

builder.Services.AddInfrastructure(builder.Configuration); // Adiciona as injeções de dependência da infraestrutura
builder.Services.AddApplication(); // Adiciona as injeções de dependência da aplicação

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Cria a rota /openapi/v1.json
    app.MapScalarApiReference(); // <--- Cria a tela visual em /scalar/v1
}

app.UseMiddleware<CultureMiddleware>(); 

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await MigrateDatabase();

app.Run();

async Task MigrateDatabase()
{
    // Cria um escopo para obter os serviços necessários
    await using var scope = app.Services.CreateAsyncScope();

    // Chama o método de migration do banco de dados. e como se fosse uma injeção de dependência
    await DatabaseMigration.MigrateDatabase(scope.ServiceProvider);
}
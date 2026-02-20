// Importação de entidades do domínio (User, Expense, etc.)
using CashFlow.Domain.Entities;
// Importação de interfaces de criptografia (para tratar senhas nos testes)
using CashFlow.Domain.Security.Cryptography;
// Importação de interfaces de tokens (para gerar tokens de autenticação para o teste)
using CashFlow.Domain.Security.Tokens;
// Importação do contexto do banco de dados real
using CashFlow.Infrastructure.DataAccess;
// Utilitários para criar entidades "fakes" (Builders)
using CommonTestUtilities.Entities;
// Dependências do ASP.NET Core para hospedar a aplicação em ambiente de teste
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace WebApi.Test;

// A classe herda de WebApplicationFactory para "copiar" a configuração da sua API (Program)
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    // Variáveis para armazenar dados que serão usados durante os testes
    private Expense _expense;
    private User _user;
    private string _password;
    private string _token;

    // Método que configura o servidor web antes de iniciar os testes
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Define que o ambiente é de "Test" (pode mudar comportamentos no Program.cs)
        builder.UseEnvironment("Test")
            .ConfigureServices(services =>
            {
                // Cria um provedor de serviços para rodar o Banco de Dados em Memória
                var provider = services.AddEntityFrameworkInMemoryDatabase().BuildServiceProvider();

                // Adiciona o contexto do banco, mas configurado para usar o "InMemoryDatabase" 
                // em vez de um banco real (como MySQL ou SQL Server)
                services.AddDbContext<CashFlowDbContext>(config =>
                {
                    config.UseInMemoryDatabase("InMemoryDbForTesting"); // Nome do banco fake
                    config.UseInternalServiceProvider(provider); // Usa o provedor criado acima
                });

                // Cria um escopo temporário para extrair serviços e manipular o banco agora
                var scope = services.BuildServiceProvider().CreateScope();
                // Obtém a instância do banco de dados fake
                var dbContext = scope.ServiceProvider.GetRequiredService<CashFlowDbContext>();
                // Obtém o serviço de criptografia de senha
                var passwordEncripter = scope.ServiceProvider.GetRequiredService<IPasswordEncripter>();

                // Chama o método para popular o banco com dados iniciais
                StartDatabase(dbContext, passwordEncripter);

                // Obtém o gerador de tokens e já gera um token válido para o usuário criado
                var tokenGenerator = scope.ServiceProvider.GetRequiredService<IAccessTokenGenerator>();
                _token = tokenGenerator.Generate(_user);
            });
    }

    // Métodos auxiliares para que as classes de teste consigam ler os dados gerados aqui
    public string GetName() => _user.Name;
    public string GetEmail() => _user.Email;
    public string GetPassword() => _password;
    public string GetToken() => _token;
    public long GetExpenseId() => _expense.Id;

    // Método que coordena a alimentação inicial do banco de dados de teste
    private void StartDatabase(CashFlowDbContext dbContext, IPasswordEncripter passwordEncripter)
    {
        AddUsers(dbContext, passwordEncripter); // Adiciona usuário
        AddExpenses(dbContext, _user);          // Adiciona despesa vinculada ao usuário

        dbContext.SaveChanges(); // Salva tudo no banco em memória
    }

    // Cria um usuário fake, criptografa a senha e salva no banco
    private void AddUsers(CashFlowDbContext dbContext, IPasswordEncripter passwordEncripter)
    {
        _user = UserBuilder.Build(); // Usa um Builder para gerar dados aleatórios/válidos
        _password = _user.Password;  // Guarda a senha limpa para usar no teste de login depois

        // Substitui a senha limpa pela versão criptografada antes de salvar no banco
        _user.Password = passwordEncripter.Encrypt(_user.Password);

        dbContext.Users.Add(_user); // Adiciona na tabela de usuários
    }

    // Cria uma despesa fake vinculada ao usuário que acabamos de criar
    private void AddExpenses(CashFlowDbContext dbContext, User user)
    {
        _expense = ExpenseBuilder.Build(user); // Gera a despesa fake

        dbContext.Expenses.Add(_expense); // Adiciona na tabela de despesas
    }
}
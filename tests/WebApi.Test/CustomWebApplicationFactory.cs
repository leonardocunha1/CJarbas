// Importação de entidades do domínio (User, Expense, etc.)
using CashFlow.Domain.Entities;
using CashFlow.Domain.Enums;

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
using WebApi.Test.Resources;

namespace WebApi.Test;

// A classe herda de WebApplicationFactory para "copiar" a configuração da sua API (Program)
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    // Variáveis para armazenar dados que serão usados durante os testes
    public ExpenseIdentityManager Expense_Admin { get; private set; } = default!;
    public ExpenseIdentityManager Expense_MemberTeam { get; private set; } = default!;
    public UserIdentityManager User_Team_Member { get; private set; } = default!;
    public UserIdentityManager User_Admin { get; private set; } = default!;

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
                // Obtém o serviço de geração de tokens (pode ser necessário para criar um token válido para os testes)
                var accessTokenGenerator = scope.ServiceProvider.GetRequiredService<IAccessTokenGenerator>();

                // Chama o método que vai alimentar o banco de dados com os dados iniciais necessários para os testes
                StartDatabase(dbContext, passwordEncripter, accessTokenGenerator);
            });
    }

    // Método que coordena a alimentação inicial do banco de dados de teste
    private void StartDatabase(
       CashFlowDbContext dbContext,
       IPasswordEncripter passwordEncripter,
       IAccessTokenGenerator accessTokenGenerator)
    {
        var userTeamMember = AddUserTeamMember(dbContext, passwordEncripter, accessTokenGenerator);
        var expenseTeamMember = AddExpenses(dbContext, userTeamMember, expenseId: 1);
        Expense_MemberTeam = new ExpenseIdentityManager(expenseTeamMember);

        var userAdmin = AddUserAdmin(dbContext, passwordEncripter, accessTokenGenerator);
        var expenseAdmin = AddExpenses(dbContext, userAdmin, expenseId: 2);
        Expense_Admin = new ExpenseIdentityManager(expenseAdmin);

        dbContext.SaveChanges(); // Salva tudo no banco em memória
    }

    // Cria um usuário fake, criptografa a senha e salva no banco
    private User AddUserTeamMember(
       CashFlowDbContext dbContext,
       IPasswordEncripter passwordEncripter,
       IAccessTokenGenerator accessTokenGenerator)
    {
        var user = UserBuilder.Build();
        user.Id = 1;

        var password = user.Password;
        user.Password = passwordEncripter.Encrypt(user.Password);

        dbContext.Users.Add(user);

        var token = accessTokenGenerator.Generate(user);

        User_Team_Member = new UserIdentityManager(user, password, token);

        return user;
    }

    private User AddUserAdmin(
        CashFlowDbContext dbContext,
        IPasswordEncripter passwordEncripter,
        IAccessTokenGenerator accessTokenGenerator)
    {
        var user = UserBuilder.Build(Roles.ADMIN);
        user.Id = 2;

        var password = user.Password;
        user.Password = passwordEncripter.Encrypt(user.Password);

        dbContext.Users.Add(user);

        var token = accessTokenGenerator.Generate(user);

        User_Admin = new UserIdentityManager(user, password, token);

        return user;
    }

    // Cria uma despesa fake vinculada ao usuário que acabamos de criar
    private Expense AddExpenses(CashFlowDbContext dbContext, User user, long expenseId)
    {
        var expense = ExpenseBuilder.Build(user); // Gera a despesa fake
        expense.Id = expenseId;

        dbContext.Expenses.Add(expense); // Adiciona na tabela de despesas

        return expense;
    }
}
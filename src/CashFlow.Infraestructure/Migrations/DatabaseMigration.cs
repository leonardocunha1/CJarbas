using CashFlow.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CashFlow.Infrastructure.Migrations;

public static class DatabaseMigration
{
    public static async Task MigrateDatabase(IServiceProvider serviceProvider)
    {
        // 1. Pede ao "Chef" (ServiceProvider) para entregar o contexto do banco de dados (DbContext)
        // Usamos GetRequiredService porque se o banco não estiver configurado, o app deve travar aqui.
        var dbContext = serviceProvider.GetRequiredService<CashFlowDbContext>();

        // 2. Este comando olha para as suas classes de "Migrations" e verifica no banco real:
        // "Tem alguma tabela nova ou alteração que ainda não foi aplicada?"
        // Se sim, ele executa os SQLs necessários para atualizar o banco.
        await dbContext.Database.MigrateAsync();
    }
}

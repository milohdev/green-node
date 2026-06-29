using Microsoft.EntityFrameworkCore;

namespace GreenNode.Infrastructure.Seeders;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(DbContext context)
    {
        if (!await context.Database.CanConnectAsync())
            throw new InvalidOperationException(
                "No se puede conectar a la base de datos. " +
                "Verifica que Docker esté corriendo.");

        await SeedTerritoriesAsync(context);
        await SeedAdminUserAsync(context);
        await SeedDemoDataAsync(context);

        await context.SaveChangesAsync();
    }

    private static async Task SeedTerritoriesAsync(DbContext context)
    {
    }

    private static async Task SeedAdminUserAsync(DbContext context)
    {
    }

    private static async Task SeedDemoDataAsync(DbContext context)
    {
    }
}

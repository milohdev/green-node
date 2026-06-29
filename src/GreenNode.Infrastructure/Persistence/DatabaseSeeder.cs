using GreenNode.Domain.Entities;
using GreenNode.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GreenNode.Infrastructure.Persistence;

public class DatabaseSeeder(ApplicationDbContext context, ILogger<DatabaseSeeder> logger)
{
    public async Task SeedAsync()
    {
        if (await context.Users.AnyAsync())
            return;

        var admin = new User(
            name: "Administrador GreenNode",
            email: "admin@greennode.co",
            passwordHash: BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            role: UserRole.Admin
        );

        var t1 = new Territory(
            name: "Bosque Andino Norte",
            department: "Antioquia",
            municipality: "Medellín",
            availableHectares: 150.50m,
            pricePerTree: 8500m
        );

        var t2 = new Territory(
            name: "Reserva Orinoquía",
            department: "Meta",
            municipality: "Villavicencio",
            availableHectares: 320.00m,
            pricePerTree: 6200m
        );

        var t3 = new Territory(
            name: "Manglar Pacífico",
            department: "Chocó",
            municipality: "Quibdó",
            availableHectares: 95.75m,
            pricePerTree: 9800m
        );

        context.Users.Add(admin);
        context.Territories.AddRange(t1, t2, t3);

        await context.SaveChangesAsync();

        logger.LogInformation("✅ Seed completado: 1 admin, 3 territorios.");
    }
}

using Microsoft.AspNetCore.Identity;
using PlataformaCreditosWeb.Models;

namespace PlataformaCreditosWeb.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var db = services.GetRequiredService<ApplicationDbContext>();

        // Crear rol Analista
        if (!await roleManager.RoleExistsAsync("Analista"))
            await roleManager.CreateAsync(new IdentityRole("Analista"));

        // Crear usuario Analista
        var emailAnalista = "analista@financiera.com";
        if (await userManager.FindByEmailAsync(emailAnalista) == null)
        {
            var analista = new ApplicationUser
            {
                UserName = emailAnalista,
                Email = emailAnalista,
                NombreCompleto = "Analista de Riesgo",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(analista, "Admin123!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(analista, "Analista");
        }

        // Crear usuarios clientes
        var emailCliente1 = "cliente1@test.com";
        var emailCliente2 = "cliente2@test.com";

        ApplicationUser? usuario1 =
            await userManager.FindByEmailAsync(emailCliente1);
        if (usuario1 == null)
        {
            usuario1 = new ApplicationUser
            {
                UserName = emailCliente1,
                Email = emailCliente1,
                NombreCompleto = "Cliente Uno",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(usuario1, "Cliente123!");
        }

        ApplicationUser? usuario2 =
            await userManager.FindByEmailAsync(emailCliente2);
        if (usuario2 == null)
        {
            usuario2 = new ApplicationUser
            {
                UserName = emailCliente2,
                Email = emailCliente2,
                NombreCompleto = "Cliente Dos",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(usuario2, "Cliente123!");
        }

        // Crear clientes y solicitudes si no existen
        if (!db.Clientes.Any())
        {
            var cliente1 = new Cliente
            {
                UsuarioId = usuario1.Id,
                IngresosMensuales = 3000,
                Activo = true
            };
            var cliente2 = new Cliente
            {
                UsuarioId = usuario2.Id,
                IngresosMensuales = 5000,
                Activo = true
            };
            db.Clientes.AddRange(cliente1, cliente2);
            await db.SaveChangesAsync();

            // Una solicitud Pendiente y una Aprobada
            db.SolicitudesCredito.AddRange(
                new SolicitudCredito
                {
                    ClienteId = cliente1.Id,
                    MontoSolicitado = 5000,
                    Estado = EstadoSolicitud.Pendiente,
                    FechaSolicitud = DateTime.UtcNow
                },
                new SolicitudCredito
                {
                    ClienteId = cliente2.Id,
                    MontoSolicitado = 10000,
                    Estado = EstadoSolicitud.Aprobado,
                    FechaSolicitud = DateTime.UtcNow.AddDays(-5)
                }
            );
            await db.SaveChangesAsync();
        }
    }
}
using GestionEntradaInventario.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GestionEntradaInventario.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{

    public DbSet<Productos> Productos { get; set; }
    public DbSet<Entradas> Entradas { get; set; }
    public DbSet<EntradasDetalle> EntradasDetalle { get; set; }
}

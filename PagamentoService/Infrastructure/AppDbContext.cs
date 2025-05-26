using Microsoft.EntityFrameworkCore;
using PagamentoService.Entities;
using PagamentoService.Models;

namespace PagamentoService.Infrastructure;
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Pagamento> Pagamentos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pagamento>().ToTable("pagamento");
    }
}


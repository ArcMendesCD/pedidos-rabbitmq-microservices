using Microsoft.EntityFrameworkCore;
using EstoqueService.Entities;

namespace EstoqueService.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ReservaEstoque> Reservas { get; set; }
        public DbSet<ReservaItem> ItensReservados { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReservaEstoque>().ToTable("reserva_estoque");
            modelBuilder.Entity<ReservaItem>().ToTable("reserva_item");

            modelBuilder.Entity<ReservaEstoque>()
                .HasMany(e => e.Itens)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}
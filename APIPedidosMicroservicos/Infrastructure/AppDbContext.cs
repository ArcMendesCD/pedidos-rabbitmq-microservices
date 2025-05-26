using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Pedido> Pedidos { get; set; }
    public DbSet<PedidoItem> ItensPedido { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pedido>()
            .Property(p => p.PedidoId)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Pedido>()
            .HasMany(p => p.Itens)
            .WithOne(pi => pi.Pedido)
            .HasForeignKey(pi => pi.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);
    }


}
using Microsoft.EntityFrameworkCore;
using PagamentoService.Infrastructure;
using PagamentoService.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql("server=localhost;database=pedidos_db;user=root;password=Andrei_01",
        new MySqlServerVersion(new Version(8, 0, 34))));

builder.Services.AddSingleton<PagamentoConsumer>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
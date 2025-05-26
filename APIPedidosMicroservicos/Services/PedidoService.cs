using Microsoft.EntityFrameworkCore;

namespace APIPedidosMicroservicos.Services;

public class PedidoService
{
    private readonly AppDbContext _dbContext;

    public PedidoService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SalvarPedidoAsync(PedidoDTO dto)
    {
        // Step 1: Cria o pedido e salva para gerar o PedidoId
        var pedido = new Pedido
        {
            ClienteId = dto.ClienteId,
            status = "pendente",
            ValorTotal = dto.ValorTotal
        };

        _dbContext.Pedidos.Add(pedido);
        await _dbContext.SaveChangesAsync(); // Agora PedidoId está disponível

        // Step 2: Cria os itens com o PedidoId correto
        var itens = dto.Itens.Select(i => new PedidoItem
        {
            ProdutoId = i.ProdutoId,
            Quantidade = i.Quantidade,
            PedidoId = pedido.PedidoId // FK corretamente atribuída
        }).ToList();

        _dbContext.ItensPedido.AddRange(itens);
        await _dbContext.SaveChangesAsync();
        
        dto.PedidoId = pedido.PedidoId;

        Console.WriteLine($"[PedidoService] Pedido {pedido.PedidoId} salvo com sucesso.");
        
    }


    public async Task AtualizarStatusPedidoAsync(int pedidoId, string novoStatus)
    {
        if (string.IsNullOrWhiteSpace(novoStatus))
        {
            Console.WriteLine($"[PedidoService] Status inválido para o pedido {pedidoId}: '{novoStatus}'");
            return;
        }

        var pedido = await _dbContext.Pedidos.FirstOrDefaultAsync(p => p.PedidoId == pedidoId);
        if (pedido == null)
        {
            Console.WriteLine($"[PedidoService] Pedido {pedidoId} não encontrado.");
            return;
        }

        pedido.status = novoStatus;
        await _dbContext.SaveChangesAsync();

        Console.WriteLine($"[PedidoService] Status do pedido {pedidoId} atualizado para '{novoStatus}'.");
    }


}
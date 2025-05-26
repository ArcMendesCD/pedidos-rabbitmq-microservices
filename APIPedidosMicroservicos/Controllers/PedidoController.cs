using APIPedidosMicroservicos.Services;
using Microsoft.AspNetCore.Mvc;

namespace APIPedidosMicroservicos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PedidoController : ControllerBase
    {
        private readonly PedidoService _pedidoService;
        private readonly PedidoPublisher _pedidoPublisher;
        
        public PedidoController(PedidoService pedidoService, PedidoPublisher pedidoPublisher)
        {
            _pedidoService = pedidoService;
            _pedidoPublisher = pedidoPublisher;
        }

        [HttpPost("criar")]
        public async Task<IActionResult> CriarPedido([FromBody] PedidoDTO pedido)
        {
            await _pedidoService.SalvarPedidoAsync(pedido);
            _pedidoPublisher.PublicarPedidoCriado(pedido);
            return Ok(new { status = "Pedido Criado, Pagamento pendente!", pedido });
        }

    }

}
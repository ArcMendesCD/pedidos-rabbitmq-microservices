using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;

public class Pedido
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PedidoId { get; set; }
    public int ClienteId { get; set; }
    public decimal ValorTotal { get; set; }
    public List<PedidoItem> Itens { get; set; }
    
    [Required]
    public string status { get; set; } = "pendente";
}

public class PedidoItem
{
    public int Id { get; set; } 
    public int ProdutoId { get; set; }
    public int Quantidade { get; set; }
    public int PedidoId { get; set; } // FK
    
    [ForeignKey("PedidoId")]
    public Pedido Pedido { get; set; }
}
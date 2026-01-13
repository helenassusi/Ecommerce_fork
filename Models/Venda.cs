using System.ComponentModel.DataAnnotations;

namespace Esse_deu_certo_MVC_Exemplo.Models
{
    public class Venda
    {
        [Key]
        public int Id { get; set; }
        public DateTime DataVenda { get; set; } = DateTime.Now;

        public decimal ValorTotal { get; set; }

        public List<ItemCarrinho> Itens { get; set; } = new(); // relacionamento com itens da venda
    }
}

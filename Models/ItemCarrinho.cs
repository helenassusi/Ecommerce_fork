using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Esse_deu_certo_MVC_Exemplo.Models
{
    public class ItemCarrinho
    {
        [Key]
        public int Id { get; set; }

        public int ProdutoId { get; set; }
        public Produto? Produto { get; set; }

        public int Quantidade { get; set; } = 1;

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        [NotMapped]
        public decimal Total => Produto != null ? Produto.Preco * Quantidade : 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Preco { get; set; }
        public string NomeProduto { get; set; } = string.Empty; //Modificar
        public string ImagemUrl { get; set; } = string.Empty; // 'Modificar
    }
}
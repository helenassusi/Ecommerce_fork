using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Esse_deu_certo_MVC_Exemplo.Models
{
    public class Produto
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do produto é obrigatório.")]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O preço é obrigatório.")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Preco { get; set; }

        [Required(ErrorMessage = "A descrição é obrigatória.")]
        [StringLength(500)]
        public string Descricao { get; set; } = string.Empty;

        [ForeignKey("CategoriaId")]
        public Categoria? Categoria { get; set; }

        [Display(Name = "URL da Imagem")]
        [StringLength(300)]
        public string ImagemUrl { get; set; } = string.Empty;

        [Display(Name = "Estoque Disponível")]
        public int QuantidadeEstoque { get; set; }

        [Display(Name = "Data de Cadastro")]
        public DateTime DataCadastro { get; set; } = DateTime.Now;
        public int QuantidadeVendida { get; set; } = 0; // novo campo
        // Adicionando a propriedade CategoriaId 
        public int CategoriaId { get; set; }
       
    }
}

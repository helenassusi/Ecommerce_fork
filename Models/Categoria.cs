using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Esse_deu_certo_MVC_Exemplo.Models
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; } = string.Empty;

        // Relacionamento com produtos
        public List<Produto>? Produtos { get; set; }
    }
}

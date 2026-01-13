using System.ComponentModel.DataAnnotations;

namespace Esse_deu_certo_MVC_Exemplo.Models
{
    public class Admin
    {
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        // Apenas para fins didáticos — em produção NÃO armazene senhas em texto puro.
        [Required]
        public string Senha { get; set; } = string.Empty;

        public bool IsSuperAdmin { get; set; } = false;
    }
}

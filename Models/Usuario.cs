using System.ComponentModel.DataAnnotations;

namespace Esse_deu_certo_MVC_Exemplo.Models
{
    public class Usuario
    {
        
            public int Id { get; set; }

            [Required]
            public string Nome { get; set; } = string.Empty;

            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            public string Senha { get; set; } = string.Empty;
            // Novo campo:
            public string Tipo { get; set; } = "Cliente"; // "Admin" ou "Cliente"

    }
}

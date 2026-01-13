using Esse_deu_certo_MVC_Exemplo.Models;
using Microsoft.EntityFrameworkCore;

namespace Esse_deu_certo_MVC_Exemplo.Data
{
    public class LojaContext : DbContext
    {
        public LojaContext(DbContextOptions<LojaContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<ItemCarrinho> ItensCarrinho { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Categoria> Categorias { get; set; } // Adicionamos esta linha
        public DbSet<Venda> Vendas { get; set; } // Adicionando DbSet para Vendas
    }
}

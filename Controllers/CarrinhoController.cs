using Microsoft.AspNetCore.Mvc;
using Esse_deu_certo_MVC_Exemplo.Models;
using Esse_deu_certo_MVC_Exemplo.Data;
using Esse_deu_certo_MVC_Exemplo.Helpers;
using System.Linq;

namespace Esse_deu_certo_MVC_Exemplo.Controllers
{
    public class CarrinhoController : Controller
    {
        private readonly LojaContext _db;
        private const string SessaoCarrinho = "Carrinho";

        public CarrinhoController(LojaContext db)
        {
            _db = db;
        }

        // Exibe carrinho (lê da sessão)
        public IActionResult Index()
        {
            var carrinho = HttpContext.Session.GetObject<List<ItemCarrinho>>(SessaoCarrinho) ?? new List<ItemCarrinho>();
            return View(carrinho);
        }

        // Adiciona produto ao carrinho e salva na sessão
        public IActionResult Adicionar(int produtoId, int quantidade = 1)
        {
            var produto = _db.Produtos.FirstOrDefault(p => p.Id == produtoId);
            if (produto == null) return NotFound();

            var carrinho = HttpContext.Session.GetObject<List<ItemCarrinho>>(SessaoCarrinho) ?? new List<ItemCarrinho>();

            var item = carrinho.FirstOrDefault(i => i.ProdutoId == produtoId);
            if (item == null)
            {
                carrinho.Add(new ItemCarrinho
                {
                    ProdutoId = produto.Id,
                    Produto = produto,
                    Quantidade = quantidade,
                    Preco = produto.Preco,
                    NomeProduto = produto.Nome,
                    ImagemUrl = produto.ImagemUrl
                });
            }
            else
            {
                item.Quantidade += quantidade;
            }

            HttpContext.Session.SetObject(SessaoCarrinho, carrinho);
            return RedirectToAction("Index");
        }

        // Remove item e salva na sessão
        public IActionResult Remover(int id)
        {
            var carrinho = HttpContext.Session.GetObject<List<ItemCarrinho>>(SessaoCarrinho) ?? new List<ItemCarrinho>();
            var item = carrinho.FirstOrDefault(i => i.ProdutoId == id);
            if (item != null)
            {
                carrinho.Remove(item);
                HttpContext.Session.SetObject(SessaoCarrinho, carrinho);
            }
            return RedirectToAction("Index");
        }

        // Limpa carrinho e remove da sessão
        public IActionResult Limpar()
        {
            HttpContext.Session.Remove(SessaoCarrinho);
            return RedirectToAction("Index");
        }
    }
}
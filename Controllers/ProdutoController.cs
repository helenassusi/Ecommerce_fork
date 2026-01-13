using Esse_deu_certo_MVC_Exemplo.Data;
using Esse_deu_certo_MVC_Exemplo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Esse_deu_certo_MVC_Exemplo.Controllers
{
    public class ProdutoController : Controller
    {
        private readonly LojaContext _db;

        public ProdutoController(LojaContext db)
        {
            _db = db;
        }

        public IActionResult Index(string busca, string categoria, string ordenacao)
        {
            var produtos = _db.Produtos.AsQueryable();

            // Filtro por busca
            if (!string.IsNullOrEmpty(busca))
            {
                produtos = produtos.Where(p => p.Nome.Contains(busca) || p.Descricao.Contains(busca));
            }

            // Filtro por categoria
            if (!string.IsNullOrEmpty(categoria))
            {
                produtos = produtos.Where(p => p.Categoria != null && p.Categoria.Nome == categoria);
            }

            // Ordenação
            produtos = ordenacao switch
            {
                "preco_asc" => produtos.OrderBy(p => p.Preco),
                "preco_desc" => produtos.OrderByDescending(p => p.Preco),
                "novo" => produtos.OrderByDescending(p => p.DataCadastro),
                _ => produtos.OrderBy(p => p.Nome)
            };

            ViewBag.Categorias = new SelectList(_db.Categorias, "Id", "Nome");
            return View(produtos.ToList());
        }

        // CREATE
        public IActionResult Create()
        {
            ViewBag.Categorias = new SelectList(_db.Categorias, "Id", "Nome");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CadastrarProduto(Produto produto)
        {
            if (ModelState.IsValid)
            {
                _db.Produtos.Add(produto);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(produto);
        }
    }
}

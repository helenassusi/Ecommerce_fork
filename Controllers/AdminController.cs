using Esse_deu_certo_MVC_Exemplo.Data;
using Esse_deu_certo_MVC_Exemplo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Esse_deu_certo_MVC_Exemplo.Controllers
{
    public class AdminController : Controller
    {
        private readonly LojaContext _db;

        public AdminController(LojaContext db)
        {
            _db = db;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UsuarioTipo") == "Admin";
        }

        // ======= DASHBOARD =======
        public IActionResult Dashboard()
        {
            var categoriasResumo = _db.Produtos
                .Include(p => p.Categoria)
                .GroupBy(p => p.Categoria != null ? p.Categoria.Nome : "Sem categoria")
                .Select(g => new CategoriaResumo
                {
                    Categoria = g.Key ?? "Sem categoria",
                    TotalProdutos = g.Count(),
                    Faturamento = g.Sum(p => p.Preco * p.QuantidadeVendida)
                })
                .ToList();

            var model = new Dashboard
            {
                Categorias = categoriasResumo,
                FaturamentoTotal = categoriasResumo.Sum(c => c.Faturamento),
                TotalProdutos = _db.Produtos.Count(),
                TotalCategorias = _db.Categorias.Count()
            };

            var totalFaturamento = _db.Vendas.Sum(v => v.ValorTotal);
            ViewBag.TotalFaturamento = totalFaturamento;

            return View(model);
        }

        // ======= USUÁRIOS =======
        public IActionResult Usuarios()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Usuario");

            var usuarios = _db.Usuarios.ToList();
            return View(usuarios);
        }

        // ======= PRODUTOS =======
        public IActionResult Produtos()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Usuario");

            var produtos = _db.Produtos.ToList();
            return View(produtos);
        }

        [HttpGet]
        public IActionResult CadastrarProduto()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Usuario");

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
                return RedirectToAction("Produtos");
            }

            return View(produto);
        }

        [HttpGet]
        public IActionResult EditarProduto(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Usuario");

            
            var produto = _db.Produtos.Find(id);
            if (produto == null) return NotFound(); //novo campo

            ViewBag.Categorias = new SelectList(_db.Categorias, "Id", "Nome", produto.CategoriaId); //novo campo
            return View(produto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarProduto(Produto produto)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Usuario");

            if (ModelState.IsValid)
            {
                _db.Produtos.Update(produto);
                _db.SaveChanges();
                return RedirectToAction("Produtos");
            }

            
            ViewBag.Categorias = new SelectList(_db.Categorias, "Id", "Nome", produto.CategoriaId); //novo campo
            return View(produto);
        }

        public IActionResult ExcluirProduto(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Usuario");

            var produto = _db.Produtos.Find(id);
            if (produto != null)
            {
                _db.Produtos.Remove(produto);
                _db.SaveChanges();
            }
            return RedirectToAction("Produtos");
        }
    }
}
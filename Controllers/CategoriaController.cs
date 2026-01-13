using Esse_deu_certo_MVC_Exemplo.Data;
using Esse_deu_certo_MVC_Exemplo.Models;
using Microsoft.AspNetCore.Mvc;

namespace Esse_deu_certo_MVC_Exemplo.Controllers
{
    public class CategoriaController : Controller
    {
        private readonly LojaContext _db;

        public CategoriaController(LojaContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var categorias = _db.Categorias.ToList();
            return View(categorias);
        }

        [HttpGet] // get request para exibir o formulário
        public IActionResult Cadastrar()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cadastrar(Categoria categoria)
        {
            if (ModelState.IsValid)
            {
                _db.Categorias.Add(categoria);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(categoria);
        }

        [HttpGet]
        public IActionResult Editar(int id)
        {
            var categoria = _db.Categorias.Find(id);
            return View(categoria);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(Categoria categoria)
        {
            if (ModelState.IsValid)
            {
                _db.Categorias.Update(categoria);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(categoria);
        }

        public IActionResult Excluir(int id)
        {
            var categoria = _db.Categorias.Find(id);
            if (categoria != null)
            {
                _db.Categorias.Remove(categoria);
                _db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}

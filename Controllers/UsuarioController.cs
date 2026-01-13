using Esse_deu_certo_MVC_Exemplo.Data;
using Esse_deu_certo_MVC_Exemplo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Esse_deu_certo_MVC_Exemplo.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly LojaContext _db;

        public UsuarioController(LojaContext db)
        {
            _db = db;
        }

        public IActionResult Login() => View(); // Ação para exibir o formulário de login

        [HttpPost] // Ação para processar o login
        public IActionResult Login(string email, string senha)
        {
            var usuario = _db.Usuarios.FirstOrDefault(u => u.Email == email && u.Senha == senha);
            if (usuario != null)
            {
                // Cria sessão (valores individuais, mantidos para compatibilidade)
                HttpContext.Session.SetString("UsuarioNome", usuario.Nome);
                HttpContext.Session.SetString("UsuarioEmail", usuario.Email);
                HttpContext.Session.SetString("UsuarioTipo", usuario.Tipo);

                // Grava também um objeto JSON contendo o Id — usado pelo CheckoutController
                var usuarioSessao = new Usuario
                {
                    Id = usuario.Id,
                    Nome = usuario.Nome,
                    Email = usuario.Email,
                    Tipo = usuario.Tipo
                };
                HttpContext.Session.SetString("UsuarioLogado", JsonConvert.SerializeObject(usuarioSessao));

                if (usuario.Tipo == "Admin")
                    return RedirectToAction("Dashboard", "Admin");
                else
                    return RedirectToAction("Index", "Produto");
            }

            ViewBag.Mensagem = "Usuário ou senha incorretos!";
            return View();

        }

        // Ação para logout = encerrar sessão
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Encerra sessão
            return RedirectToAction("Login");
        }

        public IActionResult Cadastrar() => View();

        [HttpPost]
        public IActionResult Cadastrar(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                _db.Usuarios.Add(usuario);
                _db.SaveChanges();
                return RedirectToAction("Login");
            }
            ViewBag.Mensagem = "Dados inválidos!";
            return View(usuario);
        }

        // Ação para exibir perfil do usuário
        public IActionResult Perfil()
        {
            var usuarioNome = HttpContext.Session.GetString("UsuarioNome");
            var usuarioTipo = HttpContext.Session.GetString("UsuarioTipo");
            var usuarioEmail = HttpContext.Session.GetString("UsuarioEmail"); // opcional, se você salvou no login

            if (usuarioNome == null)
            {
                // Se não houver nome de usuário na sessão, redireciona para login
                return RedirectToAction("Login", "Usuario");
            }

            // Monta um objeto Usuario apenas com os dados necessários
            var usuario = new Usuario
            {
                Nome = usuarioNome,
                Email = usuarioEmail,
                Tipo = usuarioTipo
            };

            return View(usuario);
        }

    }
}
using Esse_deu_certo_MVC_Exemplo.Data;
using Esse_deu_certo_MVC_Exemplo.Helpers;
using Esse_deu_certo_MVC_Exemplo.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Net.Mail;

namespace Esse_deu_certo_MVC_Exemplo.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly LojaContext _db;
        private const string SessaoCarrinho = "Carrinho";
        private const string SessaoUsuario = "UsuarioLogado";

        public CheckoutController(LojaContext db)
        {
            _db = db;
        }

        // View do carrinho (mostra itens e botão de confirmar)
        public IActionResult Index()
        {
            var carrinho = HttpContext.Session.GetObject<List<ItemCarrinho>>(SessaoCarrinho) ?? new List<ItemCarrinho>();
            ViewBag.Total = carrinho.Sum(i => i.Preco * i.Quantidade);
            return View(carrinho);
        }

        // Confirma a compra e salva no banco
        [HttpPost]
        public IActionResult ConfirmarCompra(string metodoPagamento = "Cartão de Crédito")
        {
            var carrinho = HttpContext.Session.GetObject<List<ItemCarrinho>>(SessaoCarrinho) ?? new List<ItemCarrinho>();

            if (!carrinho.Any())
                return RedirectToAction("Index", "Carrinho");

            // Recupera usuário logado da sessão
            var usuarioJson = HttpContext.Session.GetString(SessaoUsuario);
            if (string.IsNullOrEmpty(usuarioJson))
            {
                TempData["Mensagem"] = "Faça login para confirmar a compra.";
                return RedirectToAction("Login", "Usuario");
            }

            var usuarioSess = JsonConvert.DeserializeObject<Usuario>(usuarioJson);
            if (usuarioSess == null)
            {
                TempData["Mensagem"] = "Usuário inválido. Faça login novamente.";
                return RedirectToAction("Login", "Usuario");
            }

            var usuarioDb = _db.Usuarios.FirstOrDefault(u => u.Id == usuarioSess.Id);
            if (usuarioDb == null)
            {
                TempData["Mensagem"] = "Usuário não encontrado. Faça login novamente.";
                return RedirectToAction("Login", "Usuario");
            }

            // Cria nova venda
            var venda = new Venda
            {
                ValorTotal = carrinho.Sum(i => i.Preco * i.Quantidade),
                Itens = carrinho.Select(i => new ItemCarrinho
                {
                    ProdutoId = i.ProdutoId,
                    Quantidade = i.Quantidade,
                    Preco = i.Preco,
                    NomeProduto = i.NomeProduto,
                    ImagemUrl = i.ImagemUrl,
                    UsuarioId = usuarioDb.Id
                }).ToList(),
                DataVenda = DateTime.Now
            };

            _db.Vendas.Add(venda);

            // Atualiza estoque
            foreach (var item in venda.Itens)
            {
                var produto = _db.Produtos.FirstOrDefault(p => p.Id == item.ProdutoId);
                if (produto != null)
                {
                    produto.QuantidadeVendida += item.Quantidade;
                    produto.QuantidadeEstoque -= item.Quantidade;
                }
            }

            _db.SaveChanges();

            // Envio do e-mail com nota fiscal fictícia
            try
            {
                var pdf = GerarNotaFiscalPdf(venda, carrinho, metodoPagamento);
                EnviarEmailComAnexo(
                    destinatario: usuarioDb.Email,
                    itens: carrinho,
                    valorTotal: venda.ValorTotal,
                    metodoPagamento: metodoPagamento,
                    pdfBytes: pdf
                );
            }
            catch (Exception ex)
            {
                TempData["ErroEmail"] = "Erro ao enviar e-mail: " + ex.Message;
                Console.WriteLine(ex);
            }

            // Limpa carrinho e redireciona
            HttpContext.Session.Remove(SessaoCarrinho);
            return RedirectToAction("Confirmacao");
        }

        // Geração do PDF da nota fiscal fictícia
        private byte[] GerarNotaFiscalPdf(Venda venda, List<ItemCarrinho> itens, string metodoPagamento)
        {
            using (var ms = new MemoryStream())
            {
                var doc = new Document(PageSize.A4, 40, 40, 40, 40);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "minha_loja_logo.png");
                if (System.IO.File.Exists(logoPath))
                {
                    var logo = iTextSharp.text.Image.GetInstance(logoPath);
                    logo.Alignment = Element.ALIGN_CENTER;
                    logo.ScaleToFit(120f, 120f);
                    doc.Add(logo);
                }

                doc.Add(new Paragraph(" "));
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.BLUE);
                var normal = FontFactory.GetFont(FontFactory.HELVETICA, 12);
                var bold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);

                Paragraph titulo = new Paragraph("Nota Fiscal (Fictícia)", titleFont);
                titulo.Alignment = Element.ALIGN_CENTER;
                doc.Add(titulo);

                doc.Add(new Paragraph($"Data da Compra: {venda.DataVenda:dd/MM/yyyy HH:mm}", normal));
                doc.Add(new Paragraph($"Forma de Pagamento: {metodoPagamento}", normal));
                doc.Add(new Paragraph(" "));

                PdfPTable table = new PdfPTable(4) { WidthPercentage = 100 };
                table.SetWidths(new float[] { 50f, 15f, 10f, 25f });

                BaseColor headerColor = new BaseColor(240, 240, 240);
                AddCell(table, "Produto", bold, headerColor);
                AddCell(table, "Preço", bold, headerColor);
                AddCell(table, "Qtd", bold, headerColor);
                AddCell(table, "Total", bold, headerColor);

                foreach (var it in itens)
                {
                    AddCell(table, it.NomeProduto ?? "(sem nome)", normal);
                    AddCell(table, $"R$ {it.Preco:F2}", normal);
                    AddCell(table, it.Quantidade.ToString(), normal);
                    AddCell(table, $"R$ {(it.Preco * it.Quantidade):F2}", normal);
                }

                doc.Add(table);
                doc.Add(new Paragraph(" "));
                doc.Add(new Paragraph($"Valor Total: R$ {venda.ValorTotal:F2}", bold));
                doc.Add(new Paragraph(" "));
                doc.Add(new Paragraph("Obrigado por comprar com a Minha Loja! 😊", normal));

                doc.Close();
                return ms.ToArray();
            }
        }

        private void AddCell(PdfPTable table, string text, Font font, BaseColor bgColor = null)
        {
            var cell = new PdfPCell(new Phrase(text, font))
            {
                Padding = 5,
                BackgroundColor = bgColor
            };
            table.AddCell(cell);
        }

        // Envio de e-mail com PDF em anexo
        private void EnviarEmailComAnexo(string destinatario, List<ItemCarrinho> itens, decimal valorTotal, string metodoPagamento, byte[] pdfBytes)
        {
            // 🔐 Altere para suas credenciais
            string remetente = "rr097539@gmail.com";
            string senha = "kltsbiyijrrsbxqu"; // senha de app do Gmail
            string assunto = "Confirmação de Pedido - Minha Loja";

            string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "minha_loja_logo.png");
            string logoBase64 = "";

            if (System.IO.File.Exists(logoPath))
            {
                byte[] bytes = System.IO.File.ReadAllBytes(logoPath);
                logoBase64 = $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
            }


            string corpo = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin:auto; border:1px solid #eee; border-radius:10px; overflow:hidden'>
                <div style='background-color:#f8f9fa; text-align:center; padding:20px'>
                    <img src='{logoBase64}' alt='Minha Loja' style='height:60px'/>
                </div>
                <div style='padding:20px'>
                    <h2 style='color:#007BFF; text-align:center;'>Confirmação de Pedido</h2>
                    <p>Olá! Sua compra foi confirmada com sucesso 🎉</p>
                    <p><strong>Método de Pagamento:</strong> {metodoPagamento}</p>
                    <table style='width:100%; border-collapse:collapse; margin-top:20px'>
                        <thead>
                            <tr style='background-color:#007BFF; color:white; text-align:left'>
                                <th style='padding:8px'>Produto</th>
                                <th style='padding:8px'>Preço</th>
                                <th style='padding:8px'>Qtd</th>
                                <th style='padding:8px'>Total</th>
                            </tr>
                        </thead>
                        <tbody>";

            foreach (var item in itens)
            {
                corpo += $@"
                <tr style='border-bottom:1px solid #ddd'>
                    <td style='padding:8px'>{item.NomeProduto}</td>
                    <td style='padding:8px'>R$ {item.Preco:F2}</td>
                    <td style='padding:8px'>{item.Quantidade}</td>
                    <td style='padding:8px'>R$ {(item.Preco * item.Quantidade):F2}</td>
                </tr>";
            }

            corpo += $@"
                        </tbody>
                    </table>
                    <h3 style='text-align:right; margin-top:20px'>Total: R$ {valorTotal:F2}</h3>
                    <p>Em anexo, você encontrará a sua <strong>nota fiscal fictícia</strong>.</p>
                    <p>Obrigado por comprar conosco!</p>
                </div>
                <div style='background-color:#f8f9fa; text-align:center; padding:10px; font-size:12px; color:#777'>
                    <img src='{logoBase64}' alt='Minha Loja' style='height:40px; display:block; margin:auto'/>
                    <p style='margin-top:5px'>© {DateTime.Now.Year} Minha Loja — Todos os direitos reservados.</p>
                </div>
            </div>";

            using (MailMessage message = new MailMessage())
            {
                message.From = new MailAddress(remetente, "Minha Loja");
                message.To.Add(destinatario);
                message.Subject = assunto;
                message.Body = corpo;
                message.IsBodyHtml = true;

                if (pdfBytes != null && pdfBytes.Length > 0)
                {
                    var ms = new MemoryStream(pdfBytes);
                    message.Attachments.Add(new Attachment(ms, "NotaFiscal_MinhaLoja.pdf", "application/pdf"));
                }

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential(remetente, senha);
                    smtp.EnableSsl = true;
                    smtp.Send(message);
                }
            }
        }

        // Tela de confirmação
        public IActionResult Confirmacao()
        {
            if (TempData["ErroEmail"] != null)
                ViewBag.ErroEmail = TempData["ErroEmail"].ToString();

            return View();
        }
    }
}

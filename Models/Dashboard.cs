namespace Esse_deu_certo_MVC_Exemplo.Models
{
    public class Dashboard
    {
        public List<CategoriaResumo> Categorias { get; set; } = new();
        public decimal FaturamentoTotal { get; set; }
        public int TotalProdutos { get; set; }
        public int TotalCategorias { get; set; }
    }

    public class CategoriaResumo
    {
        public string Categoria { get; set; } = string.Empty;
        public int TotalProdutos { get; set; }
        public decimal Faturamento { get; set; }
    }
}

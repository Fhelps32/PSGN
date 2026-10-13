namespace PSGN.Presentation.ViewModels
{
    public class HomeViewModel
    {
        public ICollection<CursoItem> Cursos { get; set; } = new List<CursoItem>();
    }

    public class CursoItem
    {
        public string Nome { get; set; } = string.Empty;
        public string Coordenador { get; set; } = string.Empty;
        public ICollection<ModuloItem> Modulos = new List<ModuloItem>();
    }

    public class ModuloItem
    {
        public string Nome { get; set; } = string.Empty;
        public string IdMoodle = string.Empty;
    }
}

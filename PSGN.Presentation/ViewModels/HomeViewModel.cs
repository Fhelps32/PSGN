namespace PSGN.Presentation.ViewModels
{
    // ViewModel da tela de Cursos (HomeController -> Views/Home/Index)
    public class HomeViewModel
    {
        public string? MensagemSincronizacao { get; set; }
        public string? ErroSincronizacao { get; set; }
        public ICollection<CursoItem> Cursos { get; set; } = new List<CursoItem>();
    }

    public class CursoItem
    {
        public string Nome { get; set; } = string.Empty;
        public string Coordenador { get; set; } = string.Empty;

        public ICollection<ModulosItem> Modulos { get; set; } = new List<ModulosItem>();
    }

    public class ModulosItem
    {
        public string Nome { get; set; } = string.Empty;
        public int IdMoodle { get; set; }
    }
}

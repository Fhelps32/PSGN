namespace PSGN.Presentation.ViewModels
{
    public class HomeViewModel
    {
        public string? MensagemSincronizacao { get; set; } //REVISÃO: resumo exibido depois de clicar em "Sincronizar"
        public ICollection<CursoItem> Cursos { get; set; } = new List<CursoItem>();
    }

    public class CursoItem
    {
        public string Nome { get; set; } = string.Empty;
        public string Coordenador { get; set; } = string.Empty;

        //REVISÃO: era um campo (sem { get; set; }). Funcionava no Razor, mas model binding, validação e serialização
        //JSON ignoram campos. Virou propriedade, como o resto da classe.
        public ICollection<ModuloItem> Modulos { get; set; } = new List<ModuloItem>();
    }

    public class ModuloItem
    {
        public string Nome { get; set; } = string.Empty;

        //REVISÃO: era um campo string. Virou propriedade int, que é o tipo do IdMoodle no Modulo.
        public int IdMoodle { get; set; }
    }
}

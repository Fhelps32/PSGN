using Microsoft.AspNetCore.HttpLogging;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NSGSimulado.API.Models
{
    public class Curso
    {
        [Key]
        public int IdCurso { get; set; }

        [ForeignKey("Coordenador")]
        public int IdCoordenador { get; set; }
        public int IdMoodle { get; set; } //id que vem do moodle, para poder sincronizar os cursos do moodle com o sistema
        public string Nome { get; set; } = string.Empty;
        public string Sigla { get; set; } = string.Empty;
        public int PerLetivo { get; set; }
    }
}

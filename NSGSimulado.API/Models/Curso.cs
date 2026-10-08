using Microsoft.AspNetCore.HttpLogging;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NSGSimulado.API.Models
{
    public class Curso
    {
        [Key]
        public int IdCurso { get; set; }

        [ForeignKey("Usuario")]
        public int IdCoordenador { get; set; }
        public string IdCursoNSG { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public int PerLetivo { get; set; }

        public Usuario Usuario { get; set; }
    }
}

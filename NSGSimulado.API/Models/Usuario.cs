using System.ComponentModel.DataAnnotations;

namespace NSGSimulado.API.Models
{
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }
        public string Nome { get; set; }
        public string Matricula { get; set; }
    }
}

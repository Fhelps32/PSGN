using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSGSimulado.API.Data;
using NSGSimulado.API.Models;

namespace NSGSimulado.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : Controller
    {
        public NSGDbContext _context;
        public UsuarioController(NSGDbContext context)
        {
            _context = context;
        }

        [HttpPost("criar")]
        public IActionResult CriarUsuarios()
        {
            var usuarios = new List<Usuario>
            {
                new Usuario { Nome = "João Silva", Matricula = "123456" },
                new Usuario { Nome = "Maria Souza", Matricula = "789012" },
                new Usuario { Nome = "Carlos Oliveira", Matricula = "345678" },
                new Usuario { Nome = "Ana Santos", Matricula = "901234" },
                new Usuario { Nome = "Pedro Lima", Matricula = "567890" },
                new Usuario { Nome = "Fernanda Costa", Matricula = "234567" },
                new Usuario { Nome = "Lucas Almeida", Matricula = "890123" },
            };

            foreach (var usuario in usuarios)
            {
                if (usuario.Matricula != _context.Usuarios.FirstOrDefault(u => u.Matricula == usuario.Matricula)?.Matricula)
                {
                    _context.Usuarios.Add(usuario);
                }
            }
            _context.SaveChanges();
            return StatusCode(200);
        }

        [HttpGet("infocurso/{id}")]
        public IActionResult InfoCurso(string id)
        {
            var curso = _context.Cursos.Include(c => c.Usuario).FirstOrDefault(c => c.IdCursoNSG == id);
            if (curso == null)
            {
                return NotFound();
            }
            return Ok(curso);
        }
    }
}

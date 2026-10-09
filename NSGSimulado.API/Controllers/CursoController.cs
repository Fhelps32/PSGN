using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using NSGSimulado.API.Data;
using NSGSimulado.API.Models;

namespace NSGSimulado.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CursoController : Controller
    {
        public NSGDbContext _context;
        public CursoController(NSGDbContext context)
        {
            _context = context;
        }

        [HttpPost("criar")]
        public IActionResult CriarCursos()
        {
            var cursos = new List<Curso>
            {
                new Curso { IdCoordenador = 1, IdCursoNSG = "001", Nome = "Pós-Graduação em Educação Especial - EAD", PerLetivo = 2024 },
                new Curso { IdCoordenador = 2, IdCursoNSG = "002", Nome = "Pós-Graduação em Educação Infantil e Letramento - EAD", PerLetivo = 2024 },
                new Curso { IdCoordenador = 3, IdCursoNSG = "003", Nome = "Pós-Graduação em Gestão Integrada, Supervisão e Administração Escolar - EAD", PerLetivo = 2024 },
                new Curso { IdCoordenador = 4, IdCursoNSG = "004", Nome = "Pós-Graduação em Gestão de Projetos - EAD", PerLetivo = 2024 },
                new Curso { IdCoordenador = 5, IdCursoNSG = "005", Nome = "Pós-Graduação em Finanças - EAD", PerLetivo = 2024 },
                new Curso { IdCoordenador = 6, IdCursoNSG = "006", Nome = "Pós-Graduação em Pedagogia Empresarial - EAD", PerLetivo = 2024 }
            };

            foreach (var curso in cursos)
            {
                if (curso.IdCursoNSG != _context.Cursos.FirstOrDefault(c => c.IdCursoNSG == curso.IdCursoNSG)?.IdCursoNSG)
                {
                    _context.Cursos.Add(curso);
                }
            }
            _context.SaveChanges();
            return StatusCode(200);
        }

        [HttpGet("infocursoid/{id}")]
        public IActionResult InfoCurso(string id)
        {
            var curso = _context.Cursos.Include(c => c.Usuario).FirstOrDefault(c => c.IdCursoNSG == id);
            if (curso == null)
            {
                return NotFound();
            }
            return Ok(curso);
        }

        [HttpGet("infocursonome/{nome}")]
        public IActionResult InfoCursoPorNome(string nome)
        {
            var curso = _context.Cursos.Include(c => c.Usuario).FirstOrDefault(c => c.Nome.Contains(nome));
            if (curso == null)
            {
                return NotFound();
            }
            return Ok(curso);
        }

        public void CriarAlunos()
    }
}

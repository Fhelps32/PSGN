using Microsoft.EntityFrameworkCore;
using PSGN.Application.Data;
using PSGN.Application.Servicos.Shared;
using PSGN.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PSGN.Application.Servicos.Alunos
{
    // Leitura de alunos para o Dashboard e a página de Alunos (a sincronização fica no UsuarioService). Devolve entidades do domínio
    public class AlunoService
    {
        private readonly IPSGNDbContext _dbContext;

        public AlunoService(IPSGNDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> ContarAlunosAsync()
        {
            return await _dbContext.Alunos
                .Select(a => a.IdUsuario)
                .Distinct()
                .CountAsync();
        }

        public async Task<int> ContarAlunosCadastradosNoMesAsync(DateTime mes, int? idCurso)
        {
            var dataInicio = new DateTime(mes.Year, mes.Month, 1);
            var dataFim = dataInicio.AddMonths(1);

            var consulta = _dbContext.Alunos.AsQueryable();
            if (idCurso.HasValue)
                consulta = consulta.Where(a => a.IdCurso == idCurso.Value);

            return await consulta
                .Where(a => a.DataCadastro >= dataInicio && a.DataCadastro < dataFim)
                .CountAsync();
        }


        // Lista paginada da página de Alunos, filtrada por nome/matrícula, curso e módulo
        public async Task<PagedResult<Aluno>> ListarAlunosAsync(string? nome, int? idCurso, int? idModulo, string? ordenarPor, bool decrescente, int pagina, int tamanhoPagina)
        {
            var consulta = _dbContext.Alunos
                .Include(a => a.Usuario)
                .Include(a => a.Curso)
                .Include(a => a.Modulos).ThenInclude(am => am.Modulo)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(nome))
                consulta = consulta.Where(a => a.Usuario.Nome.Contains(nome) || a.Usuario.Matricula.Contains(nome));

            if (idCurso.HasValue)
                consulta = consulta.Where(a => a.IdCurso == idCurso.Value);

            if (idModulo.HasValue)
                consulta = consulta.Where(a => a.Modulos.Any(am => am.IdModulo == idModulo.Value));

            // Só ordena pelas colunas desta lista: o valor vem da URL
            IQueryable<Aluno> ordenada = (ordenarPor?.ToLowerInvariant(), decrescente) switch
            {
                ("curso", false) => consulta.OrderBy(a => a.Curso.Nome).ThenBy(a => a.Usuario.Nome).ThenBy(a => a.IdAluno),
                ("curso", true) => consulta.OrderByDescending(a => a.Curso.Nome).ThenBy(a => a.Usuario.Nome).ThenBy(a => a.IdAluno),
                (_, true) => consulta.OrderByDescending(a => a.Usuario.Nome).ThenBy(a => a.IdAluno),
                _ => consulta.OrderBy(a => a.Usuario.Nome).ThenBy(a => a.Curso.Nome).ThenBy(a => a.IdAluno)
            };

            return await ordenada.PaginarAsync(pagina, tamanhoPagina);
        }

        // Aluno com todas as inscrições e tentativas: painel da direita da página de Alunos (null se não existir)
        public async Task<Aluno?> ObterAlunoComHistoricoAsync(int idAluno)
        {
            return await _dbContext.Alunos
                .Include(a => a.Usuario)
                .Include(a => a.Curso).ThenInclude(c => c.Modulos)
                .Include(a => a.Modulos).ThenInclude(am => am.Modulo)
                .Include(a => a.Modulos).ThenInclude(am => am.Tentativas)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.IdAluno == idAluno);
        }
    }
}

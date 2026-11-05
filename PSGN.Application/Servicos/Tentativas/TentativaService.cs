using Microsoft.EntityFrameworkCore;
using PSGN.Application.Data;
using PSGN.Application.Interfaces.ExternoServices;
using PSGN.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PSGN.Application.Servicos.Tentativas
{
    // Ciclo de tentativas: abre a 1ª tentativa de cada inscrição, liga as provas do Moodle e executa as ações do administrador.
    // As regras ficam no domínio (Tentativa e AlunoModulo); aqui só carrega, chama a regra e grava
    public class TentativaService
    {
        private readonly IExternoTentativa _externoTentativa;
        private readonly IPSGNDbContext _dbContext;

        public TentativaService(IExternoTentativa externoTentativa, IPSGNDbContext dbContext)
        {
            _externoTentativa = externoTentativa;
            _dbContext = dbContext;
        }

        // Sincronização de um curso (chamada pelo SincronizacaoService). Devolve as tentativas abertas nesta execução
        public async Task<List<Tentativa>> SincronizarTentativasDoCursoAsync(Curso curso)
        {
            var modulos = await _dbContext.Modulos
                .Where(m => m.IdCurso == curso.IdCurso)
                .ToListAsync();

            var tentativasAbertas = new List<Tentativa>();

            foreach (var modulo in modulos)
                tentativasAbertas.AddRange(await SincronizarTentativasDoModuloAsync(modulo));

            return tentativasAbertas;
        }

        private async Task<List<Tentativa>> SincronizarTentativasDoModuloAsync(Modulo modulo)
        {
            var inscricoes = await _dbContext.AlunosModulos
                .Include(am => am.Aluno).ThenInclude(a => a.Usuario)
                .Include(am => am.Tentativas)
                .Where(am => am.IdModulo == modulo.IdModulo)
                .ToListAsync();

            // Toda inscrição sem tentativa ganha a 1ª, aberta na data de inscrição
            var tentativasAbertas = new List<Tentativa>();
            foreach (var inscricao in inscricoes.Where(am => am.Tentativas.Count == 0))
                tentativasAbertas.Add(inscricao.AbrirTentativa(inscricao.DataInscricao));

            // Provas do Moodle, só para módulos com o questionário final ligado (IdCmidMoodle != 0)
            if (modulo.IdCmidMoodle != 0)
            {
                var provas = await _externoTentativa.ObterTentativasDoQuestionarioAsync(modulo.IdCmidMoodle);
                var inscricaoPorMatricula = inscricoes.ToDictionary(am => am.Aluno.Usuario.Matricula);

                foreach (var prova in provas.Where(p => p.MatriculaAluno is not null).OrderBy(p => p.DataInicio))
                {
                    if (!inscricaoPorMatricula.TryGetValue(prova.MatriculaAluno, out var inscricao))
                        continue;

                    var jaRegistrada = inscricao.Tentativas.FirstOrDefault(t => t.IdAttempt == prova.IdAttempt);
                    var destino = jaRegistrada ?? inscricao.TentativaAberta;
                    destino?.RegistrarProva(prova.IdAttempt, prova.DataInicio, prova.DataFinal, prova.Nota);
                }
            }

            await _dbContext.SaveChangesAsync();
            return tentativasAbertas;
        }

        // Ações do administrador, chamadas pelo AlunosController. A regra do domínio lança InvalidOperationException se a ação não for permitida

        public async Task<Tentativa> AprovarAsync(int idTentativa)
        {
            var tentativa = await CarregarAsync(idTentativa);
            tentativa.Aprovar(DateTime.Now);
            await _dbContext.SaveChangesAsync();
            return tentativa;
        }

        public async Task<Tentativa> ReprovarEAbrirNovaAsync(int idTentativa)
        {
            var tentativa = await CarregarAsync(idTentativa);
            var agora = DateTime.Now;
            tentativa.Reprovar(agora);
            var nova = tentativa.AlunoModulo.AbrirTentativa(agora);
            await _dbContext.SaveChangesAsync();
            return nova;
        }

        public async Task<Tentativa> ReprovarPorInatividadeEAbrirNovaAsync(int idTentativa)
        {
            var tentativa = await CarregarAsync(idTentativa);
            var agora = DateTime.Now;
            tentativa.ReprovarPorInatividade(agora);
            var nova = tentativa.AlunoModulo.AbrirTentativa(agora);
            await _dbContext.SaveChangesAsync();
            return nova;
        }

        public async Task<Tentativa> AbrirNovaAsync(int idAlunoModulo)
        {
            var inscricao = await _dbContext.AlunosModulos
                .Include(am => am.Tentativas)
                .FirstOrDefaultAsync(am => am.IdAlunoModulo == idAlunoModulo)
                ?? throw new InvalidOperationException("Inscrição não encontrada.");
            var nova = inscricao.AbrirTentativa(DateTime.Now);
            await _dbContext.SaveChangesAsync();
            return nova;
        }

        private async Task<Tentativa> CarregarAsync(int idTentativa)
        {
            return await _dbContext.Tentativas
                .Include(t => t.AlunoModulo).ThenInclude(am => am.Tentativas)
                .FirstOrDefaultAsync(t => t.IdTentativa == idTentativa)
                ?? throw new InvalidOperationException("Tentativa não encontrada.");
        }
    }
}

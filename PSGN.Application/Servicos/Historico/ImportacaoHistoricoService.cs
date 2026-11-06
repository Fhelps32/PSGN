using Microsoft.EntityFrameworkCore;
using PSGN.Application.Data;
using PSGN.Application.Interfaces.ExternoServices;
using PSGN.Domain;

namespace PSGN.Application.Servicos.Historico
{
    // Sincronização inversa: parte das provas do Moodle (local_quizattempts_get_attempts) e monta Usuario -> Aluno -> AlunoModulo -> Tentativa
    // de quem já fez módulos antes do PSGN. Usado pelo botão "Importar histórico" da tela de Cursos (HomeController). Pode rodar várias vezes sem duplicar
    public class ImportacaoHistoricoService
    {
        private readonly IExternoTentativa _externoTentativa;
        private readonly IExternoUsuario _externoUsuario;
        private readonly IPSGNDbContext _dbContext;

        public ImportacaoHistoricoService(IExternoTentativa externoTentativa, IExternoUsuario externoUsuario, IPSGNDbContext dbContext)
        {
            _externoTentativa = externoTentativa;
            _externoUsuario = externoUsuario;
            _dbContext = dbContext;
        }

        // Percorre os módulos já cadastrados que têm o questionário final ligado (IdCmidMoodle). Devolve as tentativas criadas e as
        // matrículas que ficaram com a matrícula no lugar do nome (o Moodle não liberou a consulta de usuários ou não achou o usuário)
        public async Task<(List<Tentativa> Importadas, List<string> SemNome)> ImportarHistoricoAsync()
        {
            var modulos = await _dbContext.Modulos
                .Include(m => m.Curso)
                .Where(m => m.IdCmidMoodle != 0)
                .ToListAsync();

            var importadas = new List<Tentativa>();
            var semNome = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var nomesConsultados = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            foreach (var modulo in modulos)
            {
                var provasPorAluno = (await _externoTentativa.ObterTentativasDoQuestionarioAsync(modulo.IdCmidMoodle))
                    .Where(p => !string.IsNullOrWhiteSpace(p.MatriculaAluno))
                    .GroupBy(p => p.MatriculaAluno, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                if (provasPorAluno.Count == 0) continue;

                // O que já existe, carregado de uma vez para o módulo
                var matriculas = provasPorAluno.Select(g => g.Key).ToList();
                var usuarios = await _dbContext.Usuarios
                    .Where(u => matriculas.Contains(u.Matricula))
                    .ToDictionaryAsync(u => u.Matricula, StringComparer.OrdinalIgnoreCase);
                var alunos = await _dbContext.Alunos
                    .Where(a => a.IdCurso == modulo.IdCurso)
                    .ToDictionaryAsync(a => a.IdUsuario);
                var inscricoes = await _dbContext.AlunosModulos
                    .Include(am => am.Tentativas)
                    .Where(am => am.IdModulo == modulo.IdModulo)
                    .ToDictionaryAsync(am => am.IdAluno);

                foreach (var provasDoAluno in provasPorAluno)
                {
                    var provas = provasDoAluno.OrderBy(p => p.DataInicio).ThenBy(p => p.IdAttempt).ToList();

                    var usuario = await ObterOuCriarUsuarioAsync(provasDoAluno.Key, usuarios, nomesConsultados, semNome);

                    // Registro novo ainda tem Id 0: só se procura no que veio do banco
                    var aluno = usuario.IdUsuario != 0 && alunos.TryGetValue(usuario.IdUsuario, out var alunoExistente) ? alunoExistente : null;
                    if (aluno is null)
                    {
                        aluno = new Aluno(usuario, modulo.Curso, egresso: false);
                        _dbContext.Alunos.Add(aluno);
                    }

                    var inscricao = aluno.IdAluno != 0 && inscricoes.TryGetValue(aluno.IdAluno, out var inscricaoExistente) ? inscricaoExistente : null;
                    if (inscricao is null)
                    {
                        var primeira = provas[0];
                        inscricao = new AlunoModulo(aluno, modulo, primeira.DataInicio ?? primeira.DataFinal ?? DateTime.Now);
                        _dbContext.AlunosModulos.Add(inscricao);
                    }

                    foreach (var prova in provas)
                    {
                        var tentativa = inscricao.ImportarProva(prova.IdAttempt, prova.DataInicio, prova.DataFinal, prova.Nota);
                        if (tentativa is not null) importadas.Add(tentativa);
                    }
                }

                // Salva a cada módulo para o próximo já encontrar usuários, alunos e inscrições no banco
                await _dbContext.SaveChangesAsync();
            }

            return (importadas, semNome.ToList());
        }

        // Usuário pela matrícula; se não existir, cria com o nome do Moodle (ou com a matrícula, se o nome não vier).
        // Usuário que já ficou com a matrícula no lugar do nome tenta de novo
        private async Task<Usuario> ObterOuCriarUsuarioAsync(
            string matricula,
            Dictionary<string, Usuario> usuarios,
            Dictionary<string, string?> nomesConsultados,
            HashSet<string> semNome)
        {
            if (usuarios.TryGetValue(matricula, out var existente))
            {
                if (existente.Nome != existente.Matricula) return existente;

                var nomeReal = await ConsultarNomeAsync(matricula, nomesConsultados);
                if (nomeReal is not null) existente.Nome = nomeReal;
                else semNome.Add(matricula);
                return existente;
            }

            var nome = await ConsultarNomeAsync(matricula, nomesConsultados);
            if (nome is null) semNome.Add(matricula);

            var usuario = await _dbContext.ObterOuCriarUsuarioAsync(matricula, nome ?? matricula);
            usuarios[matricula] = usuario;
            return usuario;
        }

        // Consulta o nome no Moodle uma vez por matrícula. Erro do Moodle (função não liberada para o token) = sem nome;
        // falha de rede não é tratada aqui e chega ao controller
        private async Task<string?> ConsultarNomeAsync(string matricula, Dictionary<string, string?> nomesConsultados)
        {
            if (nomesConsultados.TryGetValue(matricula, out var nome)) return nome;

            try { nome = await _externoUsuario.ObterNomePeloUsernameAsync(matricula); }
            catch (InvalidOperationException) { nome = null; }

            nomesConsultados[matricula] = nome;
            return nome;
        }
    }
}

using Microsoft.EntityFrameworkCore; // traz ToListAsync, Include, ThenInclude e FirstOrDefaultAsync, que transformam consultas LINQ em SQL
using PSGN.Application.Data; // onde está a interface IPSGNDbContext (o acesso ao banco)
using PSGN.Application.Interfaces.ExternoServices; // onde está a interface IExternoTentativa (o acesso ao Moodle)
using PSGN.Domain; // as entidades Curso, Modulo, AlunoModulo e Tentativa
using System; // DateTime
using System.Collections.Generic; // List<T>
using System.Linq; // Where, OrderBy, GroupBy
using System.Threading.Tasks; // Task, para os métodos async

namespace PSGN.Application.Servicos.Tentativas // mesma organização dos outros services: uma pasta por assunto
{
    // REVISÃO: service reescrito para a regra de tentativas definida pelo usuário. A tentativa agora é um PERÍODO do PSGN (abre, recebe a prova,
    // é fechada pelo administrador), e não mais uma cópia de cada prova do Moodle. As regras em si ficam no domínio (Tentativa e AlunoModulo);
    // aqui só se carrega, chama a regra e grava. Os métodos devolvem entidades do domain, nunca DTO
    public class TentativaService // sincroniza as provas do Moodle e executa as ações do administrador
    {
        private readonly IExternoTentativa _externoTentativa; // quem busca as provas no Moodle (a implementação fica na Infra)
        private readonly IPSGNDbContext _dbContext; // quem lê e grava no banco do PSGN

        public TentativaService(IExternoTentativa externoTentativa, IPSGNDbContext dbContext) // o ASP.NET entrega as duas dependências aqui (injeção de dependência)
        {
            _externoTentativa = externoTentativa; // guarda o acesso ao Moodle para usar nos métodos
            _dbContext = dbContext; // guarda o acesso ao banco para usar nos métodos
        } // fim do construtor

        // ===================== SINCRONIZAÇÃO =====================

        // Devolve as tentativas ABERTAS nesta sincronização (a 1ª tentativa de cada inscrição nova ou antiga que ainda não tinha nenhuma)
        public async Task<List<Tentativa>> SincronizarTentativasDoCursoAsync(Curso curso) // recebe o Curso já carregado, então não precisa buscá-lo de novo
        {
            var modulos = await _dbContext.Modulos // começa a consulta na tabela de módulos
                .Where(m => m.IdCurso == curso.IdCurso) // REVISÃO: TODOS os módulos do curso (antes só os que tinham questionário): a 1ª tentativa abre mesmo sem questionário
                .ToListAsync(); // executa o SELECT e traz os módulos para a memória

            var tentativasAbertas = new List<Tentativa>(); // junta as tentativas abertas em todos os módulos do curso

            foreach (var modulo in modulos) // percorre os módulos um por um
                tentativasAbertas.AddRange(await SincronizarTentativasDoModuloAsync(modulo)); // sincroniza o módulo e junta as tentativas que ele abriu

            return tentativasAbertas; // devolve ao SincronizacaoService
        } // fim do método do curso

        private async Task<List<Tentativa>> SincronizarTentativasDoModuloAsync(Modulo modulo) // trabalho de um módulo só; é privado porque só este service usa
        {
            var inscricoes = await _dbContext.AlunosModulos // começa na tabela de inscrições (aluno x módulo)
                .Include(am => am.Aluno).ThenInclude(a => a.Usuario) // a matrícula, que liga a prova do Moodle ao aluno, está no Usuario
                .Include(am => am.Tentativas) // as regras de abertura e de registro da prova precisam do histórico
                .Where(am => am.IdModulo == modulo.IdModulo) // só as inscrições deste módulo
                .ToListAsync(); // uma consulta só, tudo para a memória

            // 1) Regra: "no momento em que um aluno entra em um módulo ele inicia uma tentativa". Toda inscrição sem tentativa ganha a 1ª,
            //    aberta na data em que ele entrou no módulo. Vale para as inscrições novas e para as antigas, anteriores a esta regra
            var tentativasAbertas = new List<Tentativa>(); // as que forem abertas agora
            foreach (var inscricao in inscricoes.Where(am => am.Tentativas.Count == 0)) // inscrições ainda sem nenhuma tentativa
                tentativasAbertas.Add(inscricao.AbrirTentativa(inscricao.DataInscricao)); // a regra (e o prazo de 40 dias) está no domínio

            // 2) Provas do Moodle: só para módulos com o questionário final ligado (IdCmidMoodle 0 = não dá para consultar)
            if (modulo.IdCmidMoodle != 0)
            {
                var provas = await _externoTentativa.ObterTentativasDoQuestionarioAsync(modulo.IdCmidMoodle); // todas as provas do questionário (o Moodle não filtra por aluno)
                var inscricaoPorMatricula = inscricoes.ToDictionary(am => am.Aluno.Usuario.Matricula); // achar a inscrição pela matrícula sem ir ao banco

                foreach (var prova in provas.Where(p => p.MatriculaAluno is not null).OrderBy(p => p.DataInicio)) // da mais antiga para a mais recente: no fim, vale a mais recente
                {
                    if (!inscricaoPorMatricula.TryGetValue(prova.MatriculaAluno, out var inscricao)) // a matrícula não está inscrita neste módulo no PSGN?
                        continue; // não há a quem ligar a prova

                    var jaRegistrada = inscricao.Tentativas.FirstOrDefault(t => t.IdAttempt == prova.IdAttempt); // esta prova já está em alguma tentativa?
                    var destino = jaRegistrada ?? inscricao.TentativaAberta; // senão, ela entra na tentativa aberta do aluno neste módulo
                    destino?.RegistrarProva(prova.IdAttempt, prova.DataInicio, prova.DataFinal, prova.Nota); // o domínio ignora se a tentativa já foi fechada ou se a prova é mais antiga
                } // fim do foreach das provas (sem tentativa aberta e sem registro anterior, a prova é ignorada: o módulo foi aprovado ou o administrador não abriu outra)
            } // fim do if do questionário

            await _dbContext.SaveChangesAsync(); // grava de uma vez as tentativas abertas e as provas registradas, um save por módulo
            return tentativasAbertas; // devolve as tentativas abertas neste módulo
        } // fim do método do módulo

        // ===================== AÇÕES DO ADMINISTRADOR =====================
        // Cada ação carrega a tentativa com a inscrição e o histórico, chama a regra do domínio (que lança InvalidOperationException se a ação
        // não for permitida naquele estado) e grava. A tela mostra a mensagem da exceção ao usuário

        public async Task<Tentativa> AprovarAsync(int idTentativa) // nota >= 7: fecha como aprovada e conclui o módulo
        {
            var tentativa = await CarregarAsync(idTentativa); // tentativa + inscrição + histórico
            tentativa.Aprovar(DateTime.Now); // regra no domínio
            await _dbContext.SaveChangesAsync(); // grava o fechamento e a conclusão do módulo
            return tentativa; // devolve a tentativa aprovada
        } // fim do AprovarAsync

        public async Task<Tentativa> ReprovarEAbrirNovaAsync(int idTentativa) // nota < 7: fecha como reprovada e abre a próxima no mesmo módulo
        {
            var tentativa = await CarregarAsync(idTentativa); // tentativa + inscrição + histórico
            var agora = DateTime.Now; // a mesma data fecha uma e abre a outra
            tentativa.Reprovar(agora); // regra no domínio
            var nova = tentativa.AlunoModulo.AbrirTentativa(agora); // nova tentativa, com novo prazo de 40 dias
            await _dbContext.SaveChangesAsync(); // grava as duas mudanças juntas
            return nova; // devolve a tentativa nova
        } // fim do ReprovarEAbrirNovaAsync

        public async Task<Tentativa> ReprovarPorInatividadeEAbrirNovaAsync(int idTentativa) // sem prova: fecha por inatividade e abre a próxima
        {
            var tentativa = await CarregarAsync(idTentativa); // tentativa + inscrição + histórico
            var agora = DateTime.Now; // a mesma data fecha uma e abre a outra
            tentativa.ReprovarPorInatividade(agora); // regra no domínio
            var nova = tentativa.AlunoModulo.AbrirTentativa(agora); // nova tentativa, com novo prazo de 40 dias
            await _dbContext.SaveChangesAsync(); // grava as duas mudanças juntas
            return nova; // devolve a tentativa nova
        } // fim do ReprovarPorInatividadeEAbrirNovaAsync

        public async Task<Tentativa> AbrirNovaAsync(int idAlunoModulo) // abre uma tentativa manualmente (módulo não aprovado e sem tentativa em curso)
        {
            var inscricao = await _dbContext.AlunosModulos // começa na tabela de inscrições
                .Include(am => am.Tentativas) // a regra precisa do histórico
                .FirstOrDefaultAsync(am => am.IdAlunoModulo == idAlunoModulo) // a inscrição pedida
                ?? throw new InvalidOperationException("Inscrição não encontrada."); // id que não existe (ex.: URL alterada)
            var nova = inscricao.AbrirTentativa(DateTime.Now); // regra no domínio
            await _dbContext.SaveChangesAsync(); // grava a tentativa nova
            return nova; // devolve a tentativa aberta
        } // fim do AbrirNovaAsync

        private async Task<Tentativa> CarregarAsync(int idTentativa) // tentativa com tudo de que as regras precisam
        {
            return await _dbContext.Tentativas // começa na tabela de tentativas
                .Include(t => t.AlunoModulo).ThenInclude(am => am.Tentativas) // a inscrição e o histórico (abrir a próxima exige ver as outras)
                .FirstOrDefaultAsync(t => t.IdTentativa == idTentativa) // a tentativa pedida
                ?? throw new InvalidOperationException("Tentativa não encontrada."); // id que não existe (ex.: URL alterada)
        } // fim do CarregarAsync
    } // fim da classe
} // fim do namespace

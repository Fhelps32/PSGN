using Microsoft.EntityFrameworkCore; // traz ToListAsync, Include, ThenInclude e ToDictionaryAsync, que transformam consultas LINQ em SQL
using PSGN.Application.Data; // onde está a interface IPSGNDbContext (o acesso ao banco)
using PSGN.Application.Interfaces.ExternoServices; // onde está a interface IExternoTentativa (o acesso ao Moodle)
using PSGN.Domain; // as entidades Curso, Modulo, AlunoModulo e Tentativa
using System; // TimeSpan
using System.Collections.Generic; // List<T>
using System.Linq; // Where, ToDictionary
using System.Threading.Tasks; // Task, para os métodos async

namespace PSGN.Application.Servicos.Tentativas // mesma organização dos outros services: uma pasta por assunto
{
    public class TentativaService // serviço que sincroniza as tentativas do Moodle com o banco do PSGN
    {
        private readonly IExternoTentativa _externoTentativa; // quem busca as tentativas no Moodle (a implementação fica na Infra)
        private readonly IPSGNDbContext _dbContext; // quem lê e grava no banco do PSGN

        public TentativaService(IExternoTentativa externoTentativa, IPSGNDbContext dbContext) // o ASP.NET entrega as duas dependências aqui (injeção de dependência)
        {
            _externoTentativa = externoTentativa; // guarda o acesso ao Moodle para usar nos métodos
            _dbContext = dbContext; // guarda o acesso ao banco para usar nos métodos
        } // fim do construtor

        // REVISÃO: devolve List<Tentativa> (entidade do domain) com as tentativas CRIADAS, igual o ModuloService devolve List<Modulo> e o UsuarioService devolve List<AlunoModulo>
        public async Task<List<Tentativa>> SincronizarTentativasDoCursoAsync(Curso curso) // recebe o Curso já carregado, então não precisa buscá-lo de novo
        {
            var modulos = await _dbContext.Modulos // começa a consulta na tabela de módulos
                .Where(m => m.IdCurso == curso.IdCurso && m.IdCmidMoodle != 0) // só os módulos deste curso que têm o cmid do questionário final (0 = não tem, não dá para consultar o Moodle)
                .ToListAsync(); // executa o SELECT e traz os módulos para a memória

            var tentativasCriadas = new List<Tentativa>(); // lista que vai juntar as tentativas novas de todos os módulos do curso

            foreach (var modulo in modulos) // percorre os módulos um por um
                tentativasCriadas.AddRange(await SincronizarTentativasDoModuloAsync(modulo)); // sincroniza o módulo e junta as tentativas que ele criou na lista do curso

            return tentativasCriadas; // devolve ao SincronizacaoService as tentativas novas deste curso
        } // fim do método do curso

        private async Task<List<Tentativa>> SincronizarTentativasDoModuloAsync(Modulo modulo) // trabalho de um módulo só; é privado porque só este service usa
        {
            var tentativasMoodle = await _externoTentativa.ObterTentativasDoQuestionarioAsync(modulo.IdCmidMoodle); // pergunta ao Moodle todas as tentativas do questionário final deste módulo (o Moodle não filtra por aluno)

            var tentativasCriadas = new List<Tentativa>(); // lista das tentativas novas deste módulo; é o que o método devolve

            if (tentativasMoodle.Count == 0) // se ninguém fez a prova ainda
                return tentativasCriadas; // devolve a lista vazia e evita as consultas ao banco abaixo

            var alunoModuloPorMatricula = (await _dbContext.AlunosModulos // começa na tabela de inscrições (aluno x módulo)
                    .Include(am => am.Aluno).ThenInclude(a => a.Usuario) // traz junto o Aluno e o Usuario dele, porque a matrícula está no Usuario
                    .Where(am => am.IdModulo == modulo.IdModulo) // só as inscrições deste módulo
                    .ToListAsync()) // executa uma única consulta e traz tudo para a memória
                .ToDictionary(am => am.Aluno.Usuario.Matricula); // monta um dicionário com a matrícula como chave, para achar o AlunoModulo sem ir ao banco a cada tentativa

            var tentativaPorIdAttempt = await _dbContext.Tentativas // começa na tabela de tentativas
                .Where(t => t.AlunoModulo.IdModulo == modulo.IdModulo && t.IdAttempt != 0) // só as deste módulo que vieram do Moodle (IdAttempt 0 = não veio de lá)
                .ToDictionaryAsync(t => t.IdAttempt); // dicionário com o id da tentativa no Moodle como chave, para saber o que já foi importado

            foreach (var tentativaMoodle in tentativasMoodle) // percorre cada tentativa que o Moodle devolveu
            {
                if (tentativaPorIdAttempt.TryGetValue(tentativaMoodle.IdAttempt, out var existente)) // se esta tentativa já está no banco, "existente" recebe a entidade
                {
                    if (existente.Nota != tentativaMoodle.Nota // a nota mudou no Moodle?
                        || existente.DataInicioAttempt != tentativaMoodle.DataInicio // ou a data de início mudou?
                        || existente.DataFimAttempt != tentativaMoodle.DataFinal) // ou a data de fim mudou (a prova terminou depois de importada)?
                    {
                        existente.Nota = tentativaMoodle.Nota; // copia a nota nova (o EF percebe a mudança sozinho e gera o UPDATE no SaveChanges)
                        existente.DataInicioAttempt = tentativaMoodle.DataInicio; // copia a data de início do Moodle
                        existente.DataFimAttempt = tentativaMoodle.DataFinal; // copia a data de fim do Moodle
                    } // fim da atualização; Prazo, DataInicioTentativa e DataFimTentativa não são tocados porque quem controla é o PSGN
                    continue; // já tratou esta tentativa, vai para a próxima
                } // fim do caso "já existe"

                if (tentativaMoodle.MatriculaAluno is null // a tentativa veio sem matrícula?
                    || tentativaMoodle.DataInicio is null // ou sem data de início (o construtor de Tentativa exige uma)?
                    || !alunoModuloPorMatricula.TryGetValue(tentativaMoodle.MatriculaAluno, out var alunoModulo)) // ou a matrícula não está inscrita neste módulo no PSGN? (quando acha, "alunoModulo" já recebe o vínculo)
                    continue; // não há a quem ligar a tentativa, então pula (não cria nada)

                var nova = new Tentativa(alunoModulo, tentativaMoodle.DataInicio.Value, TimeSpan.Zero) // usa o construtor do domain; DataInicioTentativa = início do attempt e Prazo = zero (o Moodle não informa nenhum dos dois)
                { // inicializador de objeto: preenche as propriedades que vêm do Moodle
                    IdAttempt = tentativaMoodle.IdAttempt, // guarda o id do Moodle, que é o que impede duplicar na próxima sincronização
                    DataInicioAttempt = tentativaMoodle.DataInicio, // início real da tentativa no Moodle
                    DataFimAttempt = tentativaMoodle.DataFinal, // fim real da tentativa (null se ainda estiver em andamento)
                    Nota = tentativaMoodle.Nota // nota da tentativa (null se ainda não foi corrigida)
                }; // fim do inicializador
                _dbContext.Tentativas.Add(nova); // marca a entidade para ser inserida no próximo SaveChanges
                tentativasCriadas.Add(nova); // guarda na lista de retorno para o chamador saber o que foi criado
            } // fim do foreach das tentativas do Moodle

            await _dbContext.SaveChangesAsync(); // grava de uma vez os INSERTs das novas e os UPDATEs das existentes alteradas, um save por módulo
            return tentativasCriadas; // devolve as tentativas novas deste módulo
        } // fim do método do módulo
    } // fim da classe
} // fim do namespace

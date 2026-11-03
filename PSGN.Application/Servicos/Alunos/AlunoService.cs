using Microsoft.EntityFrameworkCore; // CountAsync, ToListAsync, Include, ThenInclude
using PSGN.Application.Data; // IPSGNDbContext
using PSGN.Domain; // as entidades Aluno e AlunoModulo
using System; // DateTime
using System.Collections.Generic; // List<T>
using System.Linq; // Where, Select, Distinct
using System.Threading.Tasks; // Task

namespace PSGN.Application.Servicos.Alunos // pasta nova, igual à "Servicos/Alunos" do PSG
{
    // REVISÃO: service novo, só de LEITURA, usado pelo Dashboard. A sincronização dos alunos continua no UsuarioService.
    // Seguindo a sua arquitetura, os métodos devolvem entidades do domain (ou um número), nunca DTO.
    public class AlunoService // consultas sobre alunos e inscrições
    {
        private readonly IPSGNDbContext _dbContext; // acesso ao banco do PSGN

        public AlunoService(IPSGNDbContext dbContext) // o ASP.NET entrega o contexto pela injeção de dependência
        {
            _dbContext = dbContext; // guarda o contexto para usar nos métodos
        } // fim do construtor

        public async Task<int> ContarAlunosAsync() // total de alunos para o card "Total de Alunos"
        {
            return await _dbContext.Alunos // começa na tabela de alunos (1 linha por usuário em cada curso)
                .Select(a => a.IdUsuario) // pega só o usuário, porque a mesma pessoa pode ser aluna de 2 cursos
                .Distinct() // remove as repetições, para contar PESSOAS e não matrículas
                .CountAsync(); // executa um SELECT COUNT(DISTINCT ...) no banco
        } // fim do ContarAlunosAsync

        public async Task<int> ContarAlunosCadastradosNoMesAsync(DateTime mes, int? idCurso) // ponto do gráfico em linha: alunos cadastrados naquele mês (mesma regra do PSG)
        {
            var dataInicio = new DateTime(mes.Year, mes.Month, 1); // primeiro dia do mês (inclusive)
            var dataFim = dataInicio.AddMonths(1); // primeiro dia do mês seguinte (exclusive)

            var consulta = _dbContext.Alunos.AsQueryable(); // começa a consulta sem executar nada ainda
            if (idCurso.HasValue) // se a tela escolheu um curso
                consulta = consulta.Where(a => a.IdCurso == idCurso.Value); // filtra só os alunos daquele curso

            return await consulta // continua a consulta montada acima
                .Where(a => a.DataCadastro >= dataInicio && a.DataCadastro < dataFim) // só quem foi cadastrado dentro do mês
                .CountAsync(); // executa o COUNT no banco
        } // fim do ContarAlunosCadastradosNoMesAsync

        // REVISÃO: removido o ListarInscricoesAsync, que só alimentava a antiga situação das inscrições do Dashboard

        // REVISÃO: método novo, usado pela página de Alunos. Devolve entidades do domain (nunca DTO); o controller é quem monta o ViewModel
        public async Task<List<Aluno>> ListarAlunosAsync(string? nome, int? idCurso, int? idModulo) // cada filtro é opcional: null = não filtra
        {
            var consulta = _dbContext.Alunos // começa na tabela de alunos (1 linha por usuário em cada curso)
                .Include(a => a.Usuario) // traz o usuário, de onde vêm a matrícula e o nome
                .Include(a => a.Curso) // traz o curso, para mostrar o nome dele
                .Include(a => a.Modulos).ThenInclude(am => am.Modulo) // traz as inscrições do aluno e o módulo de cada uma
                .Include(a => a.Modulos).ThenInclude(am => am.Tentativas) // traz as tentativas de cada inscrição, que o AlunoModulo.ObterNotaFinal usa
                .AsNoTracking() // só leitura: o EF não precisa acompanhar mudanças, então gasta menos memória
                .AsQueryable(); // mantém como consulta para poder acrescentar filtros sem executar nada ainda

            if (!string.IsNullOrWhiteSpace(nome)) // se a tela digitou um nome
                consulta = consulta.Where(a => a.Usuario.Nome.Contains(nome)); // vira um LIKE '%nome%' no SQL Server

            if (idCurso.HasValue) // se a tela escolheu um curso
                consulta = consulta.Where(a => a.IdCurso == idCurso.Value); // só os alunos daquele curso

            if (idModulo.HasValue) // se a tela escolheu um módulo
                consulta = consulta.Where(a => a.Modulos.Any(am => am.IdModulo == idModulo.Value)); // só quem tem inscrição naquele módulo (a lista de módulos do aluno continua completa)

            return await consulta // continua a consulta montada acima
                .OrderBy(a => a.Usuario.Nome) // ordem alfabética pelo nome do aluno
                .ThenBy(a => a.Curso.Nome) // desempate: mesma pessoa em dois cursos fica agrupada e em ordem estável
                .ToListAsync(); // executa a consulta e devolve a lista de entidades
        } // fim do ListarAlunosAsync
    } // fim da classe
} // fim do namespace

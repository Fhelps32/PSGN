using Microsoft.EntityFrameworkCore;
using PSGN.Application.Data;
using PSGN.Application.Interfaces.ExternoServices;
using PSGN.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSGN.Application.Servicos.Tentativas
{
    public class TentativaService
    {
        private readonly IExternoTentativa _externoTentativa;
        private readonly IPSGNDbContext _dbContext;

        //REVISÃO: o service agora recebe o IPSGNDbContext (igual aos outros services), porque precisa gravar as tentativas
        //e procurar o AlunoModulo de cada uma. O parâmetro também foi renomeado (externoTentaiva -> externoTentativa).
        public TentativaService(IExternoTentativa externoTentativa, IPSGNDbContext dbContext)
        {
            _externoTentativa = externoTentativa;
            _dbContext = dbContext;
        }

        //REVISÃO: método novo (no arquivo original havia só "public" solto, que nem compilava). Segue o mesmo padrão do
        //ModuloService e do UsuarioService: recebe o Curso já carregado e sincroniza tudo que pertence a ele.
        public async Task<ResultadoTentativasDto> SincronizarTentativasDoCursoAsync(Curso curso)
        {
            //REVISÃO: só entram os módulos que têm IdCmidMoodle. Sem o cmid do questionário final não há como perguntar as
            //tentativas ao Moodle (o ModuloService preenche esse campo; fica 0 se o módulo não tem "Avaliação Final do Módulo").
            var modulos = await _dbContext.Modulos
                .Where(m => m.IdCurso == curso.IdCurso && m.IdCmidMoodle != 0)
                .ToListAsync();

            var total = new ResultadoTentativasDto(0, 0, 0);
            foreach (var modulo in modulos)
                total += await SincronizarTentativasDoModuloAsync(modulo);

            return total;
        }

        private async Task<ResultadoTentativasDto> SincronizarTentativasDoModuloAsync(Modulo modulo)
        {
            //REVISÃO: o Moodle é consultado pelo cmid do questionário final do módulo. Não existe como filtrar por aluno.
            var tentativasMoodle = await _externoTentativa.ObterTentativasDoQuestionarioAsync(modulo.IdCmidMoodle);
            if (tentativasMoodle.Count == 0)
                return new ResultadoTentativasDto(0, 0, 0); //ninguém fez a prova ainda: evita as consultas ao banco abaixo

            //REVISÃO: a tentativa do Moodle só traz a matrícula do aluno ("username"). O caminho no nosso banco é
            //matrícula -> Usuario -> Aluno -> AlunoModulo (do módulo). Carrego todos os AlunoModulo do módulo de uma vez
            //e indexo por matrícula, em vez de fazer uma consulta ao banco por tentativa. O ToDictionary é seguro porque
            //a matrícula é única (índice único em Usuario) e o aluno só tem uma inscrição por módulo (índice único em AlunoModulo).
            var alunoModuloPorMatricula = (await _dbContext.AlunosModulos
                    .Include(am => am.Aluno).ThenInclude(a => a.Usuario)
                    .Where(am => am.IdModulo == modulo.IdModulo)
                    .ToListAsync())
                .ToDictionary(am => am.Aluno.Usuario.Matricula);

            //REVISÃO: as tentativas deste módulo que já foram importadas, indexadas pelo id do Moodle (IdAttempt). Servem para
            //não duplicar nada quando a sincronização roda de novo. O "IdAttempt != 0" ignora tentativas que não vieram do Moodle.
            var tentativaPorIdAttempt = await _dbContext.Tentativas
                .Where(t => t.AlunoModulo.IdModulo == modulo.IdModulo && t.IdAttempt != 0)
                .ToDictionaryAsync(t => t.IdAttempt);

            int adicionadas = 0, atualizadas = 0, ignoradas = 0;

            foreach (var tentativaMoodle in tentativasMoodle)
            {
                //REVISÃO: UPSERT, e não só insert. Uma tentativa pode ser importada em andamento (sem nota nem data final)
                //e terminar depois; o professor também pode recorrigir a nota no Moodle. Se só inserisse, o PSGN guardaria
                //para sempre a primeira versão. Só os campos que vêm do Moodle são atualizados (nota e datas do attempt);
                //DataInicioTentativa, DataFimTentativa e Prazo não são tocados, para não sobrescrever o que o PSGN controla.
                if (tentativaPorIdAttempt.TryGetValue(tentativaMoodle.IdAttempt, out var existente))
                {
                    if (existente.Nota != tentativaMoodle.Nota
                        || existente.DataInicioAttempt != tentativaMoodle.DataInicio
                        || existente.DataFimAttempt != tentativaMoodle.DataFinal)
                    {
                        existente.Nota = tentativaMoodle.Nota;
                        existente.DataInicioAttempt = tentativaMoodle.DataInicio;
                        existente.DataFimAttempt = tentativaMoodle.DataFinal;
                        atualizadas++;
                    }
                    continue;
                }

                //REVISÃO: tentativa de alguém que não é aluno deste módulo no PSGN (ex.: um professor testando a prova, ou um
                //aluno que saiu da turma) não tem AlunoModulo para se ligar, então é ignorada. Em vez de pular em silêncio, ela
                //é contada em "ignoradas" e aparece no resumo da tela. Também ignora tentativa sem data de início, porque o
                //construtor de Tentativa não aceita data padrão.
                if (tentativaMoodle.MatriculaAluno is null
                    || tentativaMoodle.DataInicio is null
                    || !alunoModuloPorMatricula.TryGetValue(tentativaMoodle.MatriculaAluno, out var alunoModulo))
                {
                    ignoradas++;
                    continue;
                }

                //REVISÃO: usa o construtor do domínio (ele valida a data), que EXIGE DataInicioTentativa e Prazo. O Moodle não
                //conhece nenhum dos dois, então assumi: DataInicioTentativa = início do attempt e Prazo = TimeSpan.Zero (sem
                //prazo definido). Se Prazo/DataInicioTentativa forem vir de outra regra (ex.: NSG), é só trocar aqui.
                var nova = new Tentativa(alunoModulo, tentativaMoodle.DataInicio.Value, TimeSpan.Zero)
                {
                    IdAttempt = tentativaMoodle.IdAttempt,
                    DataInicioAttempt = tentativaMoodle.DataInicio,
                    DataFimAttempt = tentativaMoodle.DataFinal,
                    Nota = tentativaMoodle.Nota
                };
                _dbContext.Tentativas.Add(nova);
                adicionadas++;
            }

            //REVISÃO: um SaveChanges por módulo, no mesmo estilo do UsuarioService: se o Moodle falhar no módulo seguinte,
            //o que já foi importado fica salvo e a próxima sincronização continua de onde parou.
            await _dbContext.SaveChangesAsync();
            return new ResultadoTentativasDto(adicionadas, atualizadas, ignoradas);
        }
    }

    //REVISÃO: resumo da sincronização de tentativas, somado módulo a módulo (operador +) e depois curso a curso
    public record ResultadoTentativasDto(int Adicionadas, int Atualizadas, int Ignoradas)
    {
        public static ResultadoTentativasDto operator +(ResultadoTentativasDto a, ResultadoTentativasDto b)
            => new(a.Adicionadas + b.Adicionadas, a.Atualizadas + b.Atualizadas, a.Ignoradas + b.Ignoradas);
    }
}

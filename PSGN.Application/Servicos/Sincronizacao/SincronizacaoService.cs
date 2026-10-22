using PSGN.Application.Servicos.Cursos;
using PSGN.Application.Servicos.Modulos;
using PSGN.Application.Servicos.Usuarios;

namespace PSGN.Application.Servicos.Sincronizacao
{
    //REVISÃO: classe nova. A ordem da sincronização (cursos -> módulos -> alunos) é regra de negócio, então saiu do
    //HomeController e veio para a camada de Application. O controller só chama um método e mostra o resultado.
    public class SincronizacaoService
    {
        private readonly CursoService _cursoService;
        private readonly ModuloService _moduloService;
        private readonly UsuarioService _usuarioService;

        public SincronizacaoService(CursoService cursoService, ModuloService moduloService, UsuarioService usuarioService)
        {
            _cursoService = cursoService;
            _moduloService = moduloService;
            _usuarioService = usuarioService;
        }

        public async Task<ResultadoSincronizacaoDto> SincronizarTudoAsync(string idNumberCategoriaRaiz, int profundidadeCursos)
        {
            var cursosAdicionados = await _cursoService.SincronizarCursosAsync(idNumberCategoriaRaiz, profundidadeCursos);

            //REVISÃO: ESTA é a correção do problema dos módulos. Antes o HomeController só sincronizava os módulos dos cursos
            //que tinham acabado de ser CRIADOS naquela execução. Como os cursos são salvos ANTES dos módulos, bastava uma
            //execução parar no meio (erro, Moodle fora do ar, parar o debug) para os cursos ficarem gravados sem módulos. Nas
            //execuções seguintes eles já não eram "novos" e nunca mais tinham os módulos buscados. Agora todo curso cadastrado
            //passa pela sincronização de módulos e alunos, e como os serviços não duplicam nada, rodar de novo só completa o que faltou.
            var cursos = await _cursoService.ListarCursosAsync();

            var modulosAdicionados = 0;
            var inscricoesAdicionadas = 0;
            foreach (var curso in cursos)
            {
                modulosAdicionados += (await _moduloService.SincronizarModulosDoCursoAsync(curso)).Count;
                inscricoesAdicionadas += (await _usuarioService.SincronizarAlunosDoCursoAsync(curso)).Count;
            }

            return new ResultadoSincronizacaoDto(cursosAdicionados.Count(), modulosAdicionados, inscricoesAdicionadas);
        }
    }

    //REVISÃO: resumo do que foi criado, para a tela avisar o usuário
    public record ResultadoSincronizacaoDto(int CursosAdicionados, int ModulosAdicionados, int InscricoesAdicionadas);
}

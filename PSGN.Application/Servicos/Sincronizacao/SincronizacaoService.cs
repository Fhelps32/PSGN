using PSGN.Application.Servicos.Cursos; // CursoService
using PSGN.Application.Servicos.Modulos; // ModuloService
using PSGN.Application.Servicos.Tentativas; // TentativaService
using PSGN.Application.Servicos.Usuarios; // UsuarioService
using PSGN.Domain; // a entidade Curso, que é o que o método devolve

namespace PSGN.Application.Servicos.Sincronizacao // pasta do serviço que coordena os outros
{
    public class SincronizacaoService // orquestra a sequência cursos -> módulos -> alunos -> tentativas
    {
        private readonly CursoService _cursoService; // sincroniza e lista os cursos
        private readonly ModuloService _moduloService; // sincroniza os módulos de um curso
        private readonly UsuarioService _usuarioService; // sincroniza os alunos (Usuario, Aluno, AlunoModulo) de um curso
        private readonly TentativaService _tentativaService; // sincroniza as tentativas dos módulos de um curso

        public SincronizacaoService( // o ASP.NET entrega os quatro serviços pelo construtor (por isso todos estão registrados em DependencyInjection.cs)
            CursoService cursoService, // parâmetro do serviço de cursos
            ModuloService moduloService, // parâmetro do serviço de módulos
            UsuarioService usuarioService, // parâmetro do serviço de usuários/alunos
            TentativaService tentativaService) // parâmetro do serviço de tentativas
        {
            _cursoService = cursoService; // guarda para usar no método abaixo
            _moduloService = moduloService; // guarda para usar no método abaixo
            _usuarioService = usuarioService; // guarda para usar no método abaixo
            _tentativaService = tentativaService; // guarda para usar no método abaixo
        } // fim do construtor

        // REVISÃO: devolve List<Curso> (entidade do domain) em vez do ResultadoSincronizacaoDto, igual os outros services; o DTO e o ResultadoTentativasDto foram removidos
        public async Task<List<Curso>> SincronizarTudoAsync(string idNumberCategoriaRaiz, int profundidadeCursos) // recebe a categoria raiz e a profundidade usadas para achar os cursos no Moodle
        {
            await _cursoService.SincronizarCursosAsync(idNumberCategoriaRaiz, profundidadeCursos); // 1º passo: cria no banco os cursos que existem no Moodle e ainda não existem aqui

            var cursos = await _cursoService.ListarCursosAsync(); // lê TODOS os cursos do banco, não só os recém-criados, para uma execução interrompida ser completada na seguinte

            foreach (var curso in cursos) // para cada curso, executa os passos 2, 3 e 4 nesta ordem
            {
                await _moduloService.SincronizarModulosDoCursoAsync(curso); // 2º passo: cria os módulos e preenche o IdCmidMoodle (cmid do questionário final) que as tentativas precisam
                await _usuarioService.SincronizarAlunosDoCursoAsync(curso); // 3º passo: cria Usuario, Aluno e AlunoModulo; a tentativa precisa do AlunoModulo para se ligar a alguém
                await _tentativaService.SincronizarTentativasDoCursoAsync(curso); // 4º passo (por último de propósito): sem módulo+cmid e sem AlunoModulo, nenhuma tentativa seria importada
            } // fim do foreach dos cursos

            return cursos; // devolve os cursos sincronizados; a tela usa isso para montar o aviso
        } // fim do método
    } // fim da classe
} // fim do namespace

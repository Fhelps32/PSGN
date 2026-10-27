using Microsoft.Extensions.Options;
using PSGN.Application.Interfaces.ExternoServices;
using PSGN.Application.Servicos.Modulos;

namespace PSGN.Infra.Integracoes.Moodle.Modulos
{
    public class ModuloMoodleSyncService : IExternoModulo
    {
        private readonly HttpClient _httpClient;
        private readonly ConfigMoodleWebApi _configMoodleWebApi;

        //REVISÃO: antes usava ExternalServiceOptions, uma segunda classe lida da MESMA seção "ConfigMoodleWebApi" do appsettings.
        //Agora usa a ConfigMoodleWebApi como os outros serviços, que é a validada no startup ([Required] + ValidateOnStart).
        public ModuloMoodleSyncService(HttpClient httpClient, IOptions<ConfigMoodleWebApi> options)
        {
            _configMoodleWebApi = options.Value;
            _httpClient = httpClient;
        }

        public async Task<List<ModuloSaidaJsonMoodleDto>> ObterModulosPeloIdCategoriaAsync(int idCategoriaMoodle)
        {
            //REVISÃO: o POST, o tratamento de erro e a desserialização agora ficam no MoodleWebApi.ChamarAsync. Antes, se o
            //Moodle devolvesse erro, "Modulos" vinha null e o foreach estourava NullReferenceException sem dizer o motivo.
            var resposta = await MoodleWebApi.ChamarAsync<ModuloConjuntoSaidaJsonMoodleDto>(
                _httpClient, _configMoodleWebApi, "core_course_get_courses_by_field",
                new Dictionary<string, string>
                {
                    ["field"] = "category",
                    ["value"] = idCategoriaMoodle.ToString()
                });

            //REVISÃO: o foreach que copiava item por item para uma lista nova virou isso; o "?? new()" cobre a lista vir null
            return resposta.Modulos ?? new List<ModuloSaidaJsonMoodleDto>();
        }

        //REVISÃO: o nome do questionário que vale como prova do módulo virou constante (antes era um texto solto no meio do método)
        private const string NomeQuestionarioFinal = "Avaliação Final do Módulo";

        //REVISÃO: renomeado para ObterQuestionarioFinalPeloIdModuloAsync e agora devolve null quando não acha o questionário
        public async Task<ModuloQuestionarioSaidaJsonMoodleDto?> ObterQuestionarioFinalPeloIdModuloAsync(int idModuloMoodle)
        {
            var resposta = await MoodleWebApi.ChamarAsync<ModuloConjuntoQuestionarioSaidaJsonMoodleDto>(_httpClient, _configMoodleWebApi, "mod_quiz_get_quizzes_by_courses",
                new Dictionary<string, string>
                {
                    ["courseids[0]"] = idModuloMoodle.ToString()
                });

            //REVISÃO: (resolve o seu "melhorar essa saída aqui") duas falhas do FirstOrDefault antigo:
            // 1) se o módulo não tem nenhum questionário, o Moodle nem manda a chave "quizzes" e "Questionarios" vinha null,
            //    estourando NullReferenceException; o "?? new()" cobre isso.
            // 2) como o DTO é struct, FirstOrDefault devolvia um struct zerado quando não achava. O FindIndex permite devolver
            //    null de verdade (índice -1), que é o que o ModuloService precisa para saber que o módulo está sem prova.
            var questionarios = resposta.Questionarios ?? new List<ModuloQuestionarioSaidaJsonMoodleDto>();
            var indice = questionarios.FindIndex(q => q.Nome == NomeQuestionarioFinal);
            return indice >= 0 ? questionarios[indice] : null;
        }
    }
}

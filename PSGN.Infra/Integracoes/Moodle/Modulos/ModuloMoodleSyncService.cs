using Microsoft.Extensions.Options;
using PSGN.Application.Interfaces.ExternoServices;
using PSGN.Application.Servicos.Modulos;

namespace PSGN.Infra.Integracoes.Moodle.Modulos
{
    public class ModuloMoodleSyncService : IExternoModulo
    {
        private readonly HttpClient _httpClient;
        private readonly ConfigMoodleWebApi _configMoodleWebApi;

        public ModuloMoodleSyncService(HttpClient httpClient, IOptions<ConfigMoodleWebApi> options)
        {
            _configMoodleWebApi = options.Value;
            _httpClient = httpClient;
        }

        public async Task<List<ModuloSaidaJsonMoodleDto>> ObterModulosPeloIdCategoriaAsync(int idCategoriaMoodle)
        {
            var resposta = await MoodleWebApi.ChamarAsync<ModuloConjuntoSaidaJsonMoodleDto>(
                _httpClient, _configMoodleWebApi, "core_course_get_courses_by_field",
                new Dictionary<string, string>
                {
                    ["field"] = "category",
                    ["value"] = idCategoriaMoodle.ToString()
                });

            return resposta.Modulos ?? new List<ModuloSaidaJsonMoodleDto>();
        }

        // Nome do questionário que vale como prova final do módulo
        private const string NomeQuestionarioFinal = "Avaliação Final do Módulo";

        // Questionário final do módulo (null se não houver). Usado pelo ModuloService
        public async Task<ModuloQuestionarioSaidaJsonMoodleDto?> ObterQuestionarioFinalPeloIdModuloAsync(int idModuloMoodle)
        {
            var resposta = await MoodleWebApi.ChamarAsync<ModuloConjuntoQuestionarioSaidaJsonMoodleDto>(_httpClient, _configMoodleWebApi, "mod_quiz_get_quizzes_by_courses",
                new Dictionary<string, string>
                {
                    ["courseids[0]"] = idModuloMoodle.ToString()
                });

            // Sem questionários o Moodle nem manda a chave "quizzes"; FindIndex permite devolver null (o DTO é struct)
            var questionarios = resposta.Questionarios ?? new List<ModuloQuestionarioSaidaJsonMoodleDto>();
            var indice = questionarios.FindIndex(q => q.Nome == NomeQuestionarioFinal);
            return indice >= 0 ? questionarios[indice] : null;
        }
    }
}

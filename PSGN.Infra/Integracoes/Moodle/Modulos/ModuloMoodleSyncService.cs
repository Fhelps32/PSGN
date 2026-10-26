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

        public async Task<ModuloQuestionarioSaidaJsonMoodleDto> ObterQuestionarioFinalPeloIdModulo (int idModuloMoodle)
        {
            var resposta = await MoodleWebApi.ChamarAsync<ModuloConjuntoQuestionarioSaidaJsonMoodleDto>(_httpClient, _configMoodleWebApi, "mod_quiz_get_quizzes_by_courses",
                new Dictionary<string, string>
                {
                    ["courseids[0]"] = idModuloMoodle.ToString()
                });

            return resposta.Questionarios.FirstOrDefault(q => q.Nome == "Avaliação Final do Módulo"); //melhorar essa saída aqui
        }
    }
}

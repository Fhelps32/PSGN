using Microsoft.Extensions.Options;
using PSGN.Application.Interfaces.ExternoServices;
using PSGN.Application.Servicos.Cursos;

namespace PSGN.Infra.Integracoes.Moodle.Cursos
{
    public class CursoMoodleSyncService : IExternoCurso
    {
        private readonly HttpClient _httpClient;
        private readonly ConfigMoodleWebApi _configMoodleWebApi;

        public CursoMoodleSyncService(HttpClient httpClient, IOptions<ConfigMoodleWebApi> options)
        {
            _httpClient = httpClient;
            _configMoodleWebApi = options.Value;
        }

        public async Task<CursoSaidaJsonMoodleCategoriasDto?> ObterCursoPeloIdMoodleAsync(int idCurso)
        {
            //REVISÃO: o POST, o tratamento de erro e a desserialização agora ficam no MoodleWebApi.ChamarAsync
            var categorias = await MoodleWebApi.ChamarAsync<List<CursoSaidaJsonMoodleCategoriasDto>>(
                _httpClient, _configMoodleWebApi, "core_course_get_categories",
                new Dictionary<string, string>
                {
                    ["criteria[0][key]"] = "id",
                    ["criteria[0][value]"] = idCurso.ToString(),
                    ["addsubcategories"] = "0"
                });

            //REVISÃO: antes era cursoEmpacotado[0], que estoura ArgumentOutOfRangeException quando o id não existe (lista vazia)
            return categorias.Count > 0 ? categorias[0] : null;
        }

        public async Task<IEnumerable<CursoSaidaJsonMoodleCategoriasDto>> ObterCursosPeloIdNumberAsync(string idNumber, int depth)
        {
            //REVISÃO: o POST, o tratamento de erro e a desserialização agora ficam no MoodleWebApi.ChamarAsync
            var listaCursos = await MoodleWebApi.ChamarAsync<List<CursoSaidaJsonMoodleCategoriasDto>>(
                _httpClient, _configMoodleWebApi, "core_course_get_categories",
                new Dictionary<string, string>
                {
                    ["criteria[0][key]"] = "idnumber",
                    ["criteria[0][value]"] = idNumber,
                    ["addsubcategories"] = "1"
                });

            return listaCursos.Where(c => c.Depth == depth);
        }
    }
}

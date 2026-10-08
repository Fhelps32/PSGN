using Microsoft.Extensions.Options;
using PSGN.Application.Interfaces.ExternoServices;
using PSGN.Application.Servicos.Cursos;
using System.Text.Json;

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

        public async Task<IEnumerable<CursoSaidaJsonDto>> ObterCursosPeloIdNumberAsync(string idNumber, int depth)
        {
            var parametros = new Dictionary<string, string>
            {
                ["wstoken"] = _configMoodleWebApi.Token,
                ["wsfunction"] = "core_course_get_categories",
                ["moodlewsrestformat"] = "json",
                ["criteria[0][key]"] = "idnumber",
                ["criteria[0][value]"] = idNumber,
                ["addsubcategories"] = "1"
            };

            var response = _httpClient.PostAsync(_configMoodleWebApi.BaseUrl, new FormUrlEncodedContent(parametros)).Result;
            var json = response.Content.ReadAsStringAsync().Result;

            var listaCursos = JsonSerializer.Deserialize<List<CursoSaidaJsonDto>>(json).Where(c => c.Depth == depth) 
                ?? Enumerable.Empty<CursoSaidaJsonDto>();
            
            return listaCursos;
        }
    }
}

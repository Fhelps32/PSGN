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

        public async Task<CursoSaidaJsonMoodleCategoriasDto> ObterCursoPeloIdMoodle(int idCurso)
        {
            var parametros = new Dictionary<string, string>
            {
                ["wstoken"] = _configMoodleWebApi.Token,
                ["wsfunction"] = "core_course_get_categories",
                ["moodlewsrestformat"] = "json",
                ["criteria[0][key]"] = "id",
                ["criteria[0][value]"] = idCurso.ToString(),
                ["addsubcategories"] = "0"
            };

            var response = await _httpClient.PostAsync(_configMoodleWebApi.BaseUrl, new FormUrlEncodedContent(parametros));
            response.EnsureSuccessStatusCode(); //melhor forma de implementar isso?
            var json = await response.Content.ReadAsStringAsync();

            if (json.TrimStart().StartsWith("{")) //o Moodle devolve um objeto (e não uma lista) quando dá erro: token inválido, função não liberada...
                throw new InvalidOperationException($"O Moodle retornou um erro: {json}");

            var cursoEmpacotado = JsonSerializer.Deserialize<List<CursoSaidaJsonMoodleCategoriasDto>>(json) //tratar isso caso saia nulo
                ?? new List<CursoSaidaJsonMoodleCategoriasDto>();

            var curso = cursoEmpacotado[0];

            return curso;
        }

        public async Task<IEnumerable<CursoSaidaJsonMoodleCategoriasDto>> ObterCursosPeloIdNumberAsync(string idNumber, int depth)
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

            var response = await _httpClient.PostAsync(_configMoodleWebApi.BaseUrl, new FormUrlEncodedContent(parametros));
            response.EnsureSuccessStatusCode(); //melhor forma de implementar isso?
            var json = await response.Content.ReadAsStringAsync();

            if (json.TrimStart().StartsWith("{")) //o Moodle devolve um objeto (e não uma lista) quando dá erro: token inválido, função não liberada...
                throw new InvalidOperationException($"O Moodle retornou um erro: {json}");

            var listaCursos = JsonSerializer.Deserialize<List<CursoSaidaJsonMoodleCategoriasDto>>(json)
                ?? new List<CursoSaidaJsonMoodleCategoriasDto>();

            return listaCursos.Where(c => c.Depth == depth);
        }
    }
}

using Microsoft.Extensions.Options;
using PSGN.Application.Interfaces.ExternoServices;
using PSGN.Application.Servicos.Modulos;
using PSGN.Infra.Integracoes.ExternoServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PSGN.Infra.Integracoes.Moodle.Modulos
{
    public class ModuloMoodleSyncService : IExternoModulo
    {
        private readonly HttpClient _httpClient;
        private readonly ExternalServiceOptions _configMoodleWebApi;

        public ModuloMoodleSyncService(HttpClient httpClient, IOptions<ExternalServiceOptions> options)
        {
            _configMoodleWebApi = options.Value;
            _httpClient = httpClient;
        }

        public async Task<ICollection<ModuloSaidaJsonMoodleDto>> ObterModulosPeloIdCategoria(string idCategoriaMoodle)
        {
            var parametros = new Dictionary<string, string>
            {
                ["wstoken"] = _configMoodleWebApi.Token,
                ["wsfunction"] = "core_course_get_courses_by_field",
                ["moodlewsrestformat"] = "json",
                ["field"] = "category",
                ["value"] = idCategoriaMoodle,
            };

            var response = await _httpClient.PostAsync(_configMoodleWebApi.BaseUrl, new FormUrlEncodedContent(parametros));
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            //if (json.TrimStart().StartsWith("{"))  //o Moodle devolve um objeto (e não uma lista) quando dá erro: token inválido, função não liberada...
            //  throw new InvalidOperationException($"O Moodle retornou um erro: {json}");

            var listaModulosEmpacotada = JsonSerializer.Deserialize<ModuloConjuntoSaidaJsonMoodleDto>(json);
            var listaModulos = new List<ModuloSaidaJsonMoodleDto>();

            foreach(var modulo in listaModulosEmpacotada.Modulos)
            {
                listaModulos.Add(modulo);
            }

            return listaModulos;
        }
    }
}

using Microsoft.Extensions.Options;
using PSGN.Application.Interfaces.ExternoServices;
using PSGN.Application.Servicos.Usuarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PSGN.Infra.Integracoes.Moodle.Usuarios
{
    public class UsuarioMoodleSyncService : IExternoUsuario
    {
        private readonly ConfigMoodleWebApi _configMoodleWebApi;
        private readonly HttpClient _httpClient;

        public UsuarioMoodleSyncService(HttpClient httpClient, IOptions<ConfigMoodleWebApi> options)
        {
            _configMoodleWebApi = options.Value;
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<UsuarioSaidaJsonMoodleDto>> ObterUsuariosPorModuloAsync(int idCurso)
        {
            var parametros = new Dictionary<string, string>
            {
                ["wstoken"] = _configMoodleWebApi.Token,
                ["wsfunction"] = "core_enrol_get_enrolled_users",
                ["moodlewsrestformat"] = "json",
                ["courseid"] = idCurso.ToString()
            };

            var response = await _httpClient.PostAsync(_configMoodleWebApi.BaseUrl, new FormUrlEncodedContent(parametros));
            response.EnsureSuccessStatusCode(); //melhor forma de implementar isso??

            var json = await response.Content.ReadAsStringAsync();

            var listaUsuariosCru = JsonSerializer.Deserialize<List<UsuarioSaidaJsonMoodleDto>>(json)
                ?? new List<UsuarioSaidaJsonMoodleDto>();

            var listaUsuarios = listaUsuariosCru.Where(u => u.Roles.Any(r => r.Nome == "student"));

            return listaUsuarios;
        }
    }
}

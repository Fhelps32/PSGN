using Microsoft.Extensions.Options;
using PSGN.Application.Interfaces.ExternoServices;
using PSGN.Application.Servicos.Usuarios;

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

        public async Task<IEnumerable<UsuarioSaidaJsonMoodleDto>> ObterAlunosDoModuloAsync(int idModuloMoodle)
        {
            //REVISÃO: o POST, o tratamento de erro e a desserialização agora ficam no MoodleWebApi.ChamarAsync. Antes este
            //serviço não checava erro nenhum do Moodle: um token inválido virava um JsonException difícil de entender.
            var listaUsuariosCru = await MoodleWebApi.ChamarAsync<List<UsuarioSaidaJsonMoodleDto>>(
                _httpClient, _configMoodleWebApi, "core_enrol_get_enrolled_users",
                new Dictionary<string, string>
                {
                    ["courseid"] = idModuloMoodle.ToString()
                });

            //REVISÃO: "?.Any(...) == true" protege contra um usuário vir sem a lista de roles (antes daria NullReferenceException)
            return listaUsuariosCru.Where(u => u.Roles?.Any(r => r.Nome == "student") == true);
        }
    }
}

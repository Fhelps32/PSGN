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
            var listaUsuariosCru = await MoodleWebApi.ChamarAsync<List<UsuarioSaidaJsonMoodleDto>>(
                _httpClient, _configMoodleWebApi, "core_enrol_get_enrolled_users",
                new Dictionary<string, string>
                {
                    ["courseid"] = idModuloMoodle.ToString()
                });

            // Só alunos; usuário sem a lista de roles é ignorado
            return listaUsuariosCru.Where(u => u.Roles?.Any(r => r.Nome == "student") == true);
        }

        // Mesmo campo de nome da sincronização normal (firstname); lança InvalidOperationException se o token não puder usar a função
        public async Task<string?> ObterNomePeloUsernameAsync(string username)
        {
            var usuarios = await MoodleWebApi.ChamarAsync<List<UsuarioSaidaJsonMoodleDto>>(
                _httpClient, _configMoodleWebApi, "core_user_get_users_by_field",
                new Dictionary<string, string>
                {
                    ["field"] = "username",
                    ["values[0]"] = username
                });

            return usuarios.Count == 0 || string.IsNullOrWhiteSpace(usuarios[0].Nome) ? null : usuarios[0].Nome;
        }
    }
}

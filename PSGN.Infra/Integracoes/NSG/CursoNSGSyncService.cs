using PSGN.Application.Interfaces.ExternoServices;
using PSGN.Application.Servicos.Cursos;
using System.Net;
using System.Text.Json;

namespace PSGN.Infra.Integracoes.NSG
{
    public class CursoNSGSyncService : IExternoCursoNSG
    {
        private readonly HttpClient _httpClient;

        public CursoNSGSyncService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CursoSaidaJsonNSGDto?> ObterCursoAsync(string nomeCurso)
        {
            var response = await _httpClient.GetAsync($"api/curso/infocursonome/{Uri.EscapeDataString(nomeCurso)}");

            // Só o 404 significa "o NSG não conhece o curso"; os outros erros viram exceção
            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<CursoSaidaJsonNSGDto>(json);
        }
    }
}

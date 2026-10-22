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

            //REVISÃO: antes QUALQUER erro virava null ("if (!response.IsSuccessStatusCode) return null"). Um 500 do NSG
            //(erro de banco, por exemplo) fazia todos os cursos serem pulados em silêncio, sem erro nenhum na tela. Só o 404
            //significa "o NSG não conhece esse curso"; qualquer outro erro agora aparece como exceção.
            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            //REVISÃO: devolve o DTO inteiro do NSG (antes copiava só o coordenador para um DTO novo e perdia o perLetivo)
            return JsonSerializer.Deserialize<CursoSaidaJsonNSGDto>(json);
        }
    }
}

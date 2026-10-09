using PSGN.Application.Interfaces.ExternoServices;
using PSGN.Application.Servicos.Cursos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PSGN.Infra.Integracoes.NSG
{
    public class CursoNSGSyncService : IExternoCursoNSG
    {
        private readonly HttpClient _httpClient;

        public CursoNSGSyncService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CoordenadorCursoSaidaJsonDto> ObterCoordenadorDoCursoAsync(string nomeCurso)
        {
            var response = await _httpClient.GetAsync($"api/curso/infocursonome/{nomeCurso}");
            var json = await response.Content.ReadAsStringAsync();
            var cursoCoordenador = JsonSerializer.Deserialize<CursoSaidaJsonNSGDto>(json);
            var coordenadorDto = new CoordenadorCursoSaidaJsonDto
            {
                Nome = cursoCoordenador.Coordenador.Nome,
                Matricula = cursoCoordenador.Coordenador.Matricula
            };
            return coordenadorDto;
        }
    }
}

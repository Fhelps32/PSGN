using Microsoft.Extensions.Options;
using PSGN.Application.Interfaces.ExternoServices;
using PSGN.Application.Servicos.Cursos;
using PSGN.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PSGN.Infra.Integracoes.ExternoServices.Cursos
{
    public class CursoService : IExternoCurso
    {
        private readonly HttpClient _httpClient;
        private readonly ExternoServiceOpcoes _externoServiceOpcoes;

        public CursoService(HttpClient httpClient, IOptions<ExternoServiceOpcoes> externoServiceOpcoes)
        {
            _httpClient = httpClient;
            _externoServiceOpcoes = externoServiceOpcoes.Value;
        }

        public async Task<IEnumerable<CursoSaidaJsonDto>> ObterCursosPeloIdPaiAsync()
        {
            var parametros = new Dictionary<string, string>
            {
                ["wstoken"] = _externoServiceOpcoes.Token,
                ["wsfunction"] = "core_course_get_categories",
                ["moodlewsrestformat"] = "json",
                ["criteria[0][key]"] = "parent",
                ["criteria[0][value]"] = _externoServiceOpcoes.ParentCategoryId.ToString(),
                ["addsubcategories"] = "1"
            };

            var response = _httpClient.PostAsync(_externoServiceOpcoes.BaseUrl, new FormUrlEncodedContent(parametros)).Result;
            var json = response.Content.ReadAsStringAsync().Result;

            var listaCursos = JsonSerializer.Deserialize<List<CursoSaidaJsonDto>>(json).Where(c => c.IdParente == _externoServiceOpcoes.ParentCategoryId) 
                ?? Enumerable.Empty<CursoSaidaJsonDto>();
            
            return listaCursos;
        }
    }
}

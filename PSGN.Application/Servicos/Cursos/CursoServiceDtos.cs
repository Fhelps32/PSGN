using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PSGN.Application.Servicos.Cursos
{
    public struct CursoSaidaJsonMoodleCategoriasDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Nome { get; set; } = string.Empty;

        [JsonPropertyName("depth")]
        public int Depth { get; set; }

        [JsonPropertyName("idnumber")]
        public string IdNumber { get; set; } = string.Empty;

        public CursoSaidaJsonMoodleCategoriasDto()
        {
        }
    }

    public struct CursoSaidaJsonNSGDto
    {
        [JsonPropertyName("nome")]
        public string Nome { get; set; } = string.Empty;

        [JsonPropertyName("perLetivo")]
        public int PerLetivo { get; set; }

        [JsonPropertyName("usuario")]
        public CoordenadorCursoSaidaJsonDto Coordenador { get; set; }

        public CursoSaidaJsonNSGDto()
        {
        }
    }

    public struct CoordenadorCursoSaidaJsonDto
    {
        [JsonPropertyName("nome")]
        public string Nome { get; set; }

        [JsonPropertyName("matricula")]
        public string Matricula { get; set; }

        public CoordenadorCursoSaidaJsonDto()
        {
        }
    }
}

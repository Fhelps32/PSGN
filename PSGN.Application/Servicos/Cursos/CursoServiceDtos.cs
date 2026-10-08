using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PSGN.Application.Servicos.Cursos
{
    public struct CursoSaidaJsonDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Nome { get; set; } = string.Empty;

        [JsonPropertyName("depth")]
        public int Depth { get; set; }

        [JsonPropertyName("idnumber")]
        public string IdNumber { get; set; } = string.Empty;

        public CursoSaidaJsonDto(int id, string nome, int depth)
        {
            Id = id;
            Nome = nome;
            Depth = depth;
        }
    }
}

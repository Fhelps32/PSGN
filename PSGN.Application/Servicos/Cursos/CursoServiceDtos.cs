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

        [JsonPropertyName("parent")]
        public int IdParente { get; set; }

        public CursoSaidaJsonDto(int id, string nome, int idParente)
        {
            Id = id;
            Nome = nome;
            IdParente = idParente;
        }
    }
}

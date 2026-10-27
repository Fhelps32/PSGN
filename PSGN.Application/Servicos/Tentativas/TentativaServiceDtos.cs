using PSGN.Application.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PSGN.Application.Servicos.Tentativas
{
    public struct TentativaSaidaJsonMoodleDto
    {
        [JsonPropertyName("attempt_id")]
        public int IdTentativa { get; set; }

        [JsonPropertyName("username")]
        public string MatriculaAluno { get; set; }

        [JsonPropertyName("grade_attempt")]
        public double Nota { get; set; }

        [JsonPropertyName("timestart")]
        [JsonConverter(typeof(UnixTimestampDateTimeConverter))]
        public DateTime DataInicio { get; set; }

        [JsonPropertyName("timefinish")]
        [JsonConverter(typeof(UnixTimestampDateTimeConverter))]
        public DateTime DataFinal { get; set; }
    }

    public struct TentativaConjuntoSaidaJsonMoodleDto
    {
        public List<TentativaSaidaJsonMoodleDto> Tentativas { get; set; }
    }
}

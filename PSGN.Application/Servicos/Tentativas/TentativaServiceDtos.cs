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
        // Id da tentativa no Moodle (Tentativa.IdAttempt), não a chave do PSGN
        [JsonPropertyName("attempt_id")]
        public int IdAttempt { get; set; }

        // Matrícula do aluno: liga a prova ao AlunoModulo
        [JsonPropertyName("username")]
        public string MatriculaAluno { get; set; }

        // null enquanto a prova está em andamento
        [JsonPropertyName("grade_attempt")]
        public double? Nota { get; set; }

        // Anulável: o conversor devolve null para timestamp 0 ou ausente
        [JsonPropertyName("timestart")]
        [JsonConverter(typeof(UnixTimestampDateTimeConverter))]
        public DateTime? DataInicio { get; set; }

        [JsonPropertyName("timefinish")]
        [JsonConverter(typeof(UnixTimestampDateTimeConverter))]
        public DateTime? DataFinal { get; set; }
    }

    public struct TentativaConjuntoSaidaJsonMoodleDto
    {
        // O Moodle responde {"quizid":..,"attempts":[...]}
        [JsonPropertyName("attempts")]
        public List<TentativaSaidaJsonMoodleDto> Tentativas { get; set; }
    }
}

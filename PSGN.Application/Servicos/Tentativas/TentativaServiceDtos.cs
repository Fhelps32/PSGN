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
        //REVISÃO: renomeado de IdTentativa para IdAttempt. "IdTentativa" é a chave primária da entidade Tentativa no PSGN;
        //este número é o id da tentativa NO MOODLE, que no domínio se chama IdAttempt (Tentativa.IdAttempt).
        [JsonPropertyName("attempt_id")]
        public int IdAttempt { get; set; }

        //REVISÃO: o Moodle manda a matrícula do aluno no campo "username" (mesmo campo usado em UsuarioSaidaJsonMoodleDto).
        //É por ela que a tentativa é ligada ao Usuario/Aluno/AlunoModulo no banco.
        [JsonPropertyName("username")]
        public string MatriculaAluno { get; set; }

        //REVISÃO: virou double? (anulável). Uma tentativa ainda em andamento não tem nota, e o Moodle manda null; com
        //double comum o JsonSerializer estourava JsonException nesse caso.
        [JsonPropertyName("grade_attempt")]
        public double? Nota { get; set; }

        //REVISÃO: DateTime -> DateTime?. O UnixTimestampDateTimeConverter é um JsonConverter<DateTime?>; aplicado numa
        //propriedade DateTime (não anulável) o System.Text.Json lança "converter not compatible with the type". Além disso,
        //o conversor devolve null quando o timestamp é 0, o que um DateTime comum não consegue representar.
        [JsonPropertyName("timestart")]
        [JsonConverter(typeof(UnixTimestampDateTimeConverter))]
        public DateTime? DataInicio { get; set; }

        [JsonPropertyName("timefinish")]
        [JsonConverter(typeof(UnixTimestampDateTimeConverter))]
        public DateTime? DataFinal { get; set; }
    }

    public struct TentativaConjuntoSaidaJsonMoodleDto
    {
        //REVISÃO: BUG. Faltava o JsonPropertyName. O Moodle responde {"quizid":100,"attempts":[...]}; o System.Text.Json
        //diferencia maiúsculas de minúsculas e "Tentativas" nunca casa com "attempts", então a lista chegava sempre null.
        [JsonPropertyName("attempts")]
        public List<TentativaSaidaJsonMoodleDto> Tentativas { get; set; }
    }
}

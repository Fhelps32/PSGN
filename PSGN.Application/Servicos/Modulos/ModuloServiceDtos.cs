using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PSGN.Application.Servicos.Modulos
{
    public struct ModuloSaidaJsonMoodleDto
    {
        [JsonPropertyName("id")]
        public int IdMoodle { get; set; }

        [JsonPropertyName("fullname")]
        public string Nome { get; set; }

        [JsonPropertyName("categoryid")]
        public int IdCursoMoodle { get; set; }

    }

    public struct ModuloConjuntoSaidaJsonMoodleDto
    {
        [JsonPropertyName("courses")]
        public List<ModuloSaidaJsonMoodleDto> Modulos { get; set; }
    }

    public struct ModuloQuestionarioSaidaJsonMoodleDto
    {
        [JsonPropertyName("coursemodule")]
        public int IdQuestionario { get; set; }

        [JsonPropertyName("name")]
        public string Nome { get; set; }
    }

    public struct ModuloConjuntoQuestionarioSaidaJsonMoodleDto
    {
        [JsonPropertyName("quizzes")]
        public List<ModuloQuestionarioSaidaJsonMoodleDto> Questionarios { get; set; }
    }
}

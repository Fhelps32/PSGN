using PSGN.Application.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PSGN.Application.Servicos.Usuarios
{
    public struct UsuarioSaidaJsonMoodleDto
    {
        [JsonPropertyName("username")]
        public string Matricula { get; set; }

        [JsonPropertyName("firstname")]
        public string Nome { get; set; }

        //[JsonPropertyName("lastcourseaccess")]
        //[JsonConverter(typeof(UnixTimestampDateTimeConverter))]
        //public DateTime UltimoAcessoCurso { get; set; }

        //[JsonPropertyName("firstaccess")]
        //[JsonConverter(typeof(UnixTimestampDateTimeConverter))]
        //public DateTime PirmeiroAcesso { get; set; }

        //[JsonPropertyName("lastaccess")]
        //[JsonConverter(typeof(UnixTimestampDateTimeConverter))]
        //public DateTime UltimoAcesso { get; set; }

        [JsonPropertyName("roles")]
        public List<UsuarioRolesSaidaJsonMoodleDto> Roles { get; set; }
    }

    public struct UsuarioRolesSaidaJsonMoodleDto
    {
        [JsonPropertyName("shortname")]
        public string Nome { get; set; } //vamos utilizar o "shortname" como identifiicador de cada role, por que é nele que está o pdrão moodle

        [JsonPropertyName("roleid")]
        public int Id { get; set; }
    }
}

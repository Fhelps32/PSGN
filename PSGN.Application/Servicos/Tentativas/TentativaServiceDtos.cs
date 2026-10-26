using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PSGN.Application.Servicos.Tentativas
{
    public struct TentativaServiceDtos
    {
        [JsonPropertyName("attempt_id")]
        public int IdTentativa { get; set; }


    }
}

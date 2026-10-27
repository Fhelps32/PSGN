using Microsoft.Extensions.Options;
using PSGN.Application.Interfaces.ExternoServices;
using PSGN.Application.Servicos.Tentativas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSGN.Infra.Integracoes.Moodle.Tentativas
{
    //REVISÃO: implementa IExternoTentativa (a interface foi renomeada; o nome antigo tinha erro de digitação)
    public class TentativaMoodleSyncService : IExternoTentativa
    {
        private readonly HttpClient _httpClient;
        private readonly ConfigMoodleWebApi _configMoodleWebApi;

        public TentativaMoodleSyncService(HttpClient httpClient, IOptions<ConfigMoodleWebApi> options)
        {
            _httpClient = httpClient;
            _configMoodleWebApi = options.Value;
        }

        //REVISÃO: o parâmetro antes se chamava IdTentativa, mas o valor enviado ao Moodle no campo "cmid" é o id do questionário
        //(Modulo.IdCmidMoodle). Renomeado para não induzir a passar o id errado.
        public async Task<List<TentativaSaidaJsonMoodleDto>> ObterTentativasDoQuestionarioAsync(int idCmidMoodle)
        {
            var resposta = await MoodleWebApi.ChamarAsync<TentativaConjuntoSaidaJsonMoodleDto>(_httpClient, _configMoodleWebApi, "local_quizattempts_get_attempts",
                new Dictionary<string, string>
                {
                    ["cmid"] = idCmidMoodle.ToString()
                });

            //REVISÃO: antes era "return resposta.Tentativas;", que devolvia null quando a chave não casava (era o caso, por
            //causa do JsonPropertyName que faltava no DTO) e fazia o chamador estourar NullReferenceException. Agora, sem
            //tentativas, devolve uma lista vazia.
            return resposta.Tentativas ?? new List<TentativaSaidaJsonMoodleDto>();
        }
    }
}

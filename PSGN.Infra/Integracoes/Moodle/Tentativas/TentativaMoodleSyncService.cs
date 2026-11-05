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
    // Provas do Moodle (local_quizattempts_get_attempts). Usado pelo TentativaService
    public class TentativaMoodleSyncService : IExternoTentativa
    {
        private readonly HttpClient _httpClient;
        private readonly ConfigMoodleWebApi _configMoodleWebApi;

        public TentativaMoodleSyncService(HttpClient httpClient, IOptions<ConfigMoodleWebApi> options)
        {
            _httpClient = httpClient;
            _configMoodleWebApi = options.Value;
        }

        // idCmidMoodle = id do questionário (Modulo.IdCmidMoodle), enviado no campo "cmid"
        public async Task<List<TentativaSaidaJsonMoodleDto>> ObterTentativasDoQuestionarioAsync(int idCmidMoodle)
        {
            var resposta = await MoodleWebApi.ChamarAsync<TentativaConjuntoSaidaJsonMoodleDto>(_httpClient, _configMoodleWebApi, "local_quizattempts_get_attempts",
                new Dictionary<string, string>
                {
                    ["cmid"] = idCmidMoodle.ToString()
                });

            return resposta.Tentativas ?? new List<TentativaSaidaJsonMoodleDto>();
        }
    }
}

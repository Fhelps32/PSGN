using Microsoft.Extensions.Configuration;
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
    public class TentativaMoodleSyncService : IExternoTentaiva
    {
        private readonly HttpClient _httpClient;
        private readonly ConfigMoodleWebApi _configMoodleWebApi;
        
        public TentativaMoodleSyncService(HttpClient httpClient, IOptions<ConfigMoodleWebApi> options)
        {
            _httpClient = httpClient;
            _configMoodleWebApi = options.Value;
        }

        public async Task<List<TentativaSaidaJsonMoodleDto>> ObterTodasTentativasPeloIdTentativaAsync(int IdTentativa)
        {
            var resposta = await MoodleWebApi.ChamarAsync<TentativaConjuntoSaidaJsonMoodleDto>(_httpClient, _configMoodleWebApi, "local_quizattempts_get_attempts",
                new Dictionary<string, string>
                {
                    ["cmid"] = IdTentativa.ToString()
                });

            return resposta.Tentativas;
        }
    }
}

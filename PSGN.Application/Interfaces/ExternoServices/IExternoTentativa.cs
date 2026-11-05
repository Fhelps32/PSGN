using PSGN.Application.Servicos.Tentativas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSGN.Application.Interfaces.ExternoServices
{
    // Provas do Moodle. Usado pelo TentativaService na sincronização
    public interface IExternoTentativa
    {
        // Tentativas de um questionário pelo cmid (Modulo.IdCmidMoodle), não pelo id de uma tentativa
        public Task<List<TentativaSaidaJsonMoodleDto>> ObterTentativasDoQuestionarioAsync(int idCmidMoodle);
    }
}

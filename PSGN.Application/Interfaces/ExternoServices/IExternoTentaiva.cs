using PSGN.Application.Servicos.Tentativas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSGN.Application.Interfaces.ExternoServices
{
    public interface IExternoTentaiva
    {
        public Task<List<TentativaSaidaJsonMoodleDto>> ObterTodasTentativasPeloIdTentativaAsync(int IdTentativa);
    }
}

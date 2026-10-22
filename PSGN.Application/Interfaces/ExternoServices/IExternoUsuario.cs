using PSGN.Application.Servicos.Usuarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSGN.Application.Interfaces.ExternoServices
{
    public interface IExternoUsuario
    {
        //REVISÃO: renomeado de ObterUsuariosPorModuloAsync. O nome agora diz que só vêm alunos (a implementação já filtrava
        //"student") e que o id é o do MÓDULO no Moodle, que foi exatamente o que estava sendo passado errado.
        public Task<IEnumerable<UsuarioSaidaJsonMoodleDto>> ObterAlunosDoModuloAsync(int idModuloMoodle);
    }
}

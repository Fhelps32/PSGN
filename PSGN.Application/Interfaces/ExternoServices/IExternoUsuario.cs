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
        // Alunos (papel "student") inscritos num módulo do Moodle. Usado pelo UsuarioService
        public Task<IEnumerable<UsuarioSaidaJsonMoodleDto>> ObterAlunosDoModuloAsync(int idModuloMoodle);

        // Nome do usuário pelo username (matrícula); null se o Moodle não o encontrar. Usado pelo ImportacaoHistoricoService
        public Task<string?> ObterNomePeloUsernameAsync(string username);
    }
}

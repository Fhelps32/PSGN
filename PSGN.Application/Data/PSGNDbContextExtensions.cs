using Microsoft.EntityFrameworkCore;
using PSGN.Domain;

namespace PSGN.Application.Data
{
    //REVISÃO: classe nova. O "procura o usuário pela matrícula e, se não existir, cria" estava repetido no CursoService e no
    //UsuarioService, cada um de um jeito, e os dois podiam criar o mesmo usuário duas vezes. Agora a regra fica num lugar só.
    public static class PSGNDbContextExtensions
    {
        public static async Task<Usuario> ObterOuCriarUsuarioAsync(this IPSGNDbContext dbContext, string matricula, string nome)
        {
            //REVISÃO: o "Local" são as entidades que o EF já está rastreando nesta requisição, INCLUSIVE as que receberam Add()    
            //e ainda não foram salvas. Uma consulta normal (Any/FirstOrDefault) vai no banco e não enxerga essas. Era isso que
            //fazia o mesmo usuário ser criado de novo dentro do mesmo loop. Primeiro olha na memória, depois no banco.
            var usuario = dbContext.Usuarios.Local.FirstOrDefault(u => u.Matricula == matricula)
                ?? await dbContext.Usuarios.FirstOrDefaultAsync(u => u.Matricula == matricula);

            if (usuario is null)
            {
                usuario = new Usuario(matricula, nome);
                dbContext.Usuarios.Add(usuario);
            }

            return usuario;
        }
    }
}

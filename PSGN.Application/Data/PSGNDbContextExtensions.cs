using Microsoft.EntityFrameworkCore;
using PSGN.Domain;

namespace PSGN.Application.Data
{
    // Busca o usuário pela matrícula ou cria. Usado pelo CursoService (coordenadores) e pelo UsuarioService (alunos)
    public static class PSGNDbContextExtensions
    {
        public static async Task<Usuario> ObterOuCriarUsuarioAsync(this IPSGNDbContext dbContext, string matricula, string nome)
        {
            // Procura primeiro no que já foi adicionado e não salvo (Local), depois no banco: evita criar o mesmo usuário duas vezes no mesmo loop
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

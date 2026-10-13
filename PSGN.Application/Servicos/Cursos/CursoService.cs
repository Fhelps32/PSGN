using PSGN.Application.Data;
using PSGN.Application.Interfaces.ExternoServices;
using PSGN.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSGN.Application.Servicos.Cursos
{
    public class CursoService
    {
        private readonly IExternoCurso _externoCurso;
        private readonly IPSGNDbContext _dbContext;
        private readonly IExternoCursoNSG _externoCursoNSG; 

        public CursoService(IExternoCurso externoCurso, IPSGNDbContext dbContext, IExternoCursoNSG externoCursoNSG)
        {
            _externoCurso = externoCurso;
            _dbContext = dbContext;
            _externoCursoNSG = externoCursoNSG;
        }

        public async Task<IEnumerable<Curso>> SincronizarCursosAsync(string idNumber, int profundidade)
        {
            var cursosExternos = await _externoCurso.ObterCursosPeloIdNumberAsync(idNumber, profundidade);
            var cursosInternos = _dbContext.Cursos.ToList();
            var cursosParaAdicionar = cursosExternos //todos os cursos novos que vão ser cadastrados
                .Where(cursoExterno => !cursosInternos.Any(cursoInterno => cursoInterno.Nome == cursoExterno.Nome));

            var cursosAdicionados = new List<Curso>(); //lista pra retornar no final da função

            foreach (var curso in cursosParaAdicionar)
            {
                var coordenadorNSG = await _externoCursoNSG.ObterCoordenadorDoCursoAsync(curso.Nome); //verifica qual coordenador pertence a esse curso
                if (coordenadorNSG is null)
                    continue; //o NSG não conhece esse curso, então não temos coordenador pra ele: pula pro próximo

                var coordenadorDto = coordenadorNSG.Value;
                if (_dbContext.Usuarios.Any(u => u.Matricula == coordenadorDto.Matricula))
                {
                    //se o coordenador já está cadastrado ele apenas cria o curso e adiciona o coordenador
                    var usuarioCoordenador = _dbContext.Usuarios.FirstOrDefault(u => u.Matricula == coordenadorDto.Matricula);
                    var novoCurso = new Curso(curso.Nome, curso.Nome, curso.Id, usuarioCoordenador!);
                    _dbContext.Cursos.Add(novoCurso);

                    cursosAdicionados.Add(novoCurso); //adiciona na lista de pra retornar no final da função
                }
                else
                {
                    //se o coordenador não estiver cadastrado: cadastramos primeiro e criamos o curso depois
                    var novoUsuario = new Usuario(coordenadorDto.Matricula, coordenadorDto.Nome); //cria o coordenador = Usuario
                    _dbContext.Usuarios.Add(novoUsuario);
                    await _dbContext.SaveChangesAsync();
                    var novoCurso = new Curso(curso.Nome, curso.Nome, curso.Id, novoUsuario!);
                    _dbContext.Cursos.Add(novoCurso);

                    cursosAdicionados.Add(novoCurso);//adiciona na lista de pra retornar no final da função
                }
            }
            await _dbContext.SaveChangesAsync(); //grava no banco todos os cursos adicionados no loop
            return cursosAdicionados;
        }
    }
}

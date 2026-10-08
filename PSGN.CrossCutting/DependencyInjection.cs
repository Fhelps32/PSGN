using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PSGN.Application.Interfaces.ExternoServices;
using PSGN.Infra.Data;
using PSGN.Infra.Integracoes.Moodle.Cursos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSGN.CrossCutting
{
    public static class DependencyInjection
    {
        #region Application

        #endregion

        #region Infra
        public static IServiceCollection AddInfraData(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Não foi possível obter a string de conexão.");
            services.AddDbContext<PSGNDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });
            return services;

            services.AddScoped<IExternoCurso, CursoMoodleSyncService>();
        }
        #endregion
    }
}

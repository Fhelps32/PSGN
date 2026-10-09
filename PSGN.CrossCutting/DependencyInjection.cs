using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PSGN.Application.Interfaces.ExternoServices;
using PSGN.Infra.Data;
using PSGN.Infra.Integracoes.Moodle.Cursos;
using PSGN.Infra.Integracoes.NSG;
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
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient<IExternoCurso, CursoMoodleSyncService>(client =>
            {
                client.BaseAddress = new Uri(configuration["Moodle:BaseUrl"] ?? throw new ArgumentNullException("Não foi possível obter a URL base do Moodle."));
            });

            return services;
        }
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
        }
        #endregion
    }
}

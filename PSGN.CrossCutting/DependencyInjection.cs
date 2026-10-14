using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PSGN.Application.Data;
using PSGN.Application.Interfaces.ExternoServices;
using PSGN.Application.Servicos.Cursos;
using PSGN.Application.Servicos.Modulos;
using PSGN.Application.Servicos.Usuarios;
using PSGN.Infra.Data;
using PSGN.Infra.Integracoes.ExternoServices;
using PSGN.Infra.Integracoes.Moodle.Cursos;
using PSGN.Infra.Integracoes.Moodle.Modulos;
using PSGN.Infra.Integracoes.Moodle.Usuarios;
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
            services.AddScoped<CursoService>();
            services.AddScoped<ModuloService>();
            services.AddScoped<UsuarioService>();
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
            services.AddScoped<IPSGNDbContext>(provider => provider.GetRequiredService<PSGNDbContext>());

            services.AddHttpClient<IExternoCurso, CursoMoodleSyncService>();
            services.AddHttpClient<IExternoCursoNSG, CursoNSGSyncService>(client =>
            {
                client.BaseAddress = new Uri(configuration["ConfigNSGApi:BaseUrl"]
                    ?? throw new ArgumentNullException("Não foi possível obter a URL base da API do NSG."));
            });
            services.AddHttpClient<IExternoModulo, ModuloMoodleSyncService>();
            services.AddHttpClient<IExternoUsuario, UsuarioMoodleSyncService>();

            services.Configure<ExternalServiceOptions>(configuration.GetSection("ConfigMoodleWebApi"));

            return services;
        }
        #endregion
    }
}

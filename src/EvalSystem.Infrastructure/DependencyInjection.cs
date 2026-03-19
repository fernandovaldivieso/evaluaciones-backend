using EvalSystem.Application.Interfaces;
using EvalSystem.Domain.Interfaces;
using EvalSystem.Infrastructure.Persistence;
using EvalSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EvalSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<EvalSystemDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<ITecnologiaService, TecnologiaService>();
        services.AddScoped<IEvaluacionService, EvaluacionService>();
        services.AddScoped<IProcesoService, ProcesoService>();
        services.AddScoped<ISesionService, SesionService>();
        services.AddScoped<IResultadoService, ResultadoService>();

        return services;
    }
}

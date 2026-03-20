using EvalSystem.API.Hubs;
using EvalSystem.Domain.Enums;
using EvalSystem.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace EvalSystem.API.BackgroundServices;

public class SesionExpirationService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<SesionHub> _hubContext;
    private readonly ILogger<SesionExpirationService> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(30);

    public SesionExpirationService(IServiceScopeFactory scopeFactory, IHubContext<SesionHub> hubContext,
        ILogger<SesionExpirationService> logger)
    {
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SesionExpirationService iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckExpiredSessions(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en SesionExpirationService.");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task CheckExpiredSessions(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EvalSystemDbContext>();

        var now = DateTime.UtcNow;

        // Find sessions that are EnProgreso and have exceeded their time limit
        var sesionesExpiradas = await db.SesionesEvaluacion
            .Include(s => s.Evaluacion)
            .Include(s => s.Respuestas)
            .Where(s => s.Estado == EstadoSesion.EnProgreso
                && s.FechaInicio.HasValue
                && s.Evaluacion.TiempoLimiteMinutos > 0)
            .ToListAsync(ct);

        foreach (var sesion in sesionesExpiradas)
        {
            var limiteExpiracion = sesion.FechaInicio!.Value.AddMinutes(sesion.Evaluacion.TiempoLimiteMinutos);
            if (now <= limiteExpiracion)
            {
                // Send warning if less than 2 minutes remaining
                var tiempoRestante = (limiteExpiracion - now).TotalSeconds;
                if (tiempoRestante <= 120 && tiempoRestante > 90)
                {
                    await _hubContext.Clients.Group($"sesion-{sesion.Id}")
                        .SendAsync("TiempoAdvertencia", new
                        {
                            sesionId = sesion.Id,
                            tiempoRestanteSegundos = (int)tiempoRestante,
                            mensaje = "Quedan menos de 2 minutos para que expire la sesión."
                        }, ct);
                }
                continue;
            }

            // Expire the session
            sesion.Estado = EstadoSesion.Expirada;
            sesion.FechaFin = limiteExpiracion;
            sesion.ScoreObtenido = sesion.Respuestas.Sum(r => r.PuntajeObtenido ?? 0);

            _logger.LogInformation("Sesión {SesionId} expirada automáticamente.", sesion.Id);

            // Notify connected client
            await _hubContext.Clients.Group($"sesion-{sesion.Id}")
                .SendAsync("SesionExpirada", new
                {
                    sesionId = sesion.Id,
                    scoreObtenido = sesion.ScoreObtenido,
                    scoreMaximo = sesion.ScoreMaximo,
                    mensaje = "Tu sesión de evaluación ha expirado por tiempo."
                }, ct);
        }

        if (sesionesExpiradas.Any())
        {
            await db.SaveChangesAsync(ct);
        }
    }
}

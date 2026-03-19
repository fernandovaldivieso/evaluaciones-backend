using System.Text.Json;
using EvalSystem.Application.Common;
using EvalSystem.Application.DTOs.Resultados;
using EvalSystem.Application.Interfaces;
using EvalSystem.Domain.Entities;
using EvalSystem.Domain.Enums;
using EvalSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EvalSystem.Infrastructure.Services;

public class ResultadoService : IResultadoService
{
    private readonly IRepository<ResultadoEvaluacion> _resultadoRepo;
    private readonly IRepository<SesionEvaluacion> _sesionRepo;
    private readonly IRepository<ProcesoSeleccion> _procesoRepo;
    private readonly IUnitOfWork _uow;

    public ResultadoService(IRepository<ResultadoEvaluacion> resultadoRepo,
        IRepository<SesionEvaluacion> sesionRepo, IRepository<ProcesoSeleccion> procesoRepo, IUnitOfWork uow)
    {
        _resultadoRepo = resultadoRepo;
        _sesionRepo = sesionRepo;
        _procesoRepo = procesoRepo;
        _uow = uow;
    }

    public async Task<ApiResponse<ResultadoDto>> AnalizarAsync(Guid sesionId)
    {
        var sesion = await _sesionRepo.Query()
            .Include(s => s.Candidato)
            .Include(s => s.Evaluacion).ThenInclude(e => e.Secciones).ThenInclude(sec => sec.Preguntas)
            .Include(s => s.Respuestas)
            .FirstOrDefaultAsync(s => s.Id == sesionId);

        if (sesion is null) return ApiResponse<ResultadoDto>.NotFound("Sesión no encontrada.");
        if (sesion.Estado != EstadoSesion.Finalizada)
            return ApiResponse<ResultadoDto>.BadRequest("La sesión debe estar finalizada para generar resultados.");

        var existente = await _resultadoRepo.Query().FirstOrDefaultAsync(r => r.SesionId == sesionId);
        if (existente is not null)
            return ApiResponse<ResultadoDto>.Conflict("Ya existe un resultado para esta sesión.");

        var scoresPorSeccion = new Dictionary<string, object>();
        var fortalezas = new List<string>();
        var brechas = new List<string>();

        foreach (var seccion in sesion.Evaluacion.Secciones)
        {
            var preguntaIds = seccion.Preguntas.Select(p => p.Id).ToHashSet();
            var respuestasSeccion = sesion.Respuestas.Where(r => preguntaIds.Contains(r.PreguntaId)).ToList();

            var maxSeccion = seccion.Preguntas.Sum(p => p.Puntaje);
            var obtenidoSeccion = respuestasSeccion.Sum(r => r.PuntajeObtenido ?? 0);
            var porcentaje = maxSeccion > 0 ? Math.Round((decimal)obtenidoSeccion / maxSeccion * 100, 2) : 0;

            scoresPorSeccion[seccion.Nombre] = new { Obtenido = obtenidoSeccion, Maximo = maxSeccion, Porcentaje = porcentaje };

            if (porcentaje >= 70) fortalezas.Add(seccion.Nombre);
            else brechas.Add(seccion.Nombre);
        }

        var scoreTotal = sesion.ScoreMaximo > 0
            ? Math.Round((decimal)(sesion.ScoreObtenido ?? 0) / sesion.ScoreMaximo * 100, 2) : 0;

        var recomendacion = scoreTotal switch
        {
            >= 90 => "Candidato(a) con excelente dominio. Altamente recomendado(a) para el puesto.",
            >= 70 => "Candidato(a) con buen nivel. Considerar para el puesto con plan de desarrollo en áreas de mejora.",
            >= 50 => "Candidato(a) con nivel intermedio. Requiere desarrollo significativo en áreas débiles.",
            _ => "Candidato(a) necesita formación adicional antes de ser considerado(a) para el puesto."
        };

        var resultado = new ResultadoEvaluacion
        {
            ScoreTotal = scoreTotal,
            ScorePorSeccion = JsonSerializer.Serialize(scoresPorSeccion),
            BrechasIdentificadas = brechas.Count > 0 ? string.Join(", ", brechas) : null,
            FortalezasIdentificadas = fortalezas.Count > 0 ? string.Join(", ", fortalezas) : null,
            RecomendacionIA = recomendacion,
            FechaAnalisis = DateTime.UtcNow,
            SesionId = sesionId
        };

        await _resultadoRepo.AddAsync(resultado);
        await _uow.SaveChangesAsync();

        return ApiResponse<ResultadoDto>.Created(ToDto(resultado, sesion.Candidato.Nombre, sesion.Evaluacion.Nombre),
            "Análisis generado exitosamente.");
    }

    public async Task<ApiResponse<ResultadoDto>> GetBySesionAsync(Guid sesionId)
    {
        var resultado = await _resultadoRepo.Query()
            .Include(r => r.Sesion).ThenInclude(s => s.Candidato)
            .Include(r => r.Sesion).ThenInclude(s => s.Evaluacion)
            .FirstOrDefaultAsync(r => r.SesionId == sesionId);

        if (resultado is null) return ApiResponse<ResultadoDto>.NotFound("Resultado no encontrado.");
        return ApiResponse<ResultadoDto>.Ok(ToDto(resultado, resultado.Sesion.Candidato.Nombre, resultado.Sesion.Evaluacion.Nombre));
    }

    public async Task<ApiResponse<RankingProcesoDto>> GetRankingAsync(Guid procesoId)
    {
        var proceso = await _procesoRepo.GetByIdAsync(procesoId);
        if (proceso is null) return ApiResponse<RankingProcesoDto>.NotFound("Proceso no encontrado.");

        var sesiones = await _sesionRepo.Query()
            .Where(s => s.ProcesoId == procesoId && s.Estado == EstadoSesion.Finalizada)
            .Include(s => s.Candidato)
            .Include(s => s.Resultado)
            .Where(s => s.Resultado != null)
            .ToListAsync();

        var ranking = sesiones
            .GroupBy(s => s.CandidatoId)
            .Select(g =>
            {
                var sesionesC = g.ToList();
                var nombre = sesionesC.First().Candidato.Nombre;
                var avgScore = sesionesC.Average(s => s.Resultado!.ScoreTotal);
                var fortalezas = sesionesC.Where(s => s.Resultado!.FortalezasIdentificadas is not null)
                    .Select(s => s.Resultado!.FortalezasIdentificadas!).FirstOrDefault();
                var gaps = sesionesC.Where(s => s.Resultado!.BrechasIdentificadas is not null)
                    .Select(s => s.Resultado!.BrechasIdentificadas!).FirstOrDefault();
                return new { CandidatoId = g.Key, Nombre = nombre, Score = Math.Round(avgScore, 2), Fortalezas = fortalezas, Brechas = gaps };
            })
            .OrderByDescending(x => x.Score)
            .Select((x, i) => new ComparacionCandidatoDto(x.CandidatoId, x.Nombre, x.Score, x.Fortalezas, x.Brechas, i + 1))
            .ToList();

        return ApiResponse<RankingProcesoDto>.Ok(new RankingProcesoDto(procesoId, proceso.Nombre, ranking));
    }

    private static ResultadoDto ToDto(ResultadoEvaluacion r, string candidatoNombre, string evaluacionNombre) => new(
        r.Id, r.ScoreTotal, r.ScorePorSeccion, r.BrechasIdentificadas,
        r.FortalezasIdentificadas, r.RecomendacionIA, r.FechaAnalisis,
        r.SesionId, candidatoNombre, evaluacionNombre);
}

using System.Text.Json;
using EvalSystem.Application.Common;
using EvalSystem.Application.DTOs.Resultados;
using EvalSystem.Application.DTOs.Sesiones;
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
    private readonly IRepository<RespuestaCandidato> _respuestaRepo;
    private readonly IRepository<ProcesoSeleccion> _procesoRepo;
    private readonly IAnalisisIAService _iaService;
    private readonly IUnitOfWork _uow;

    public ResultadoService(IRepository<ResultadoEvaluacion> resultadoRepo,
        IRepository<SesionEvaluacion> sesionRepo, IRepository<RespuestaCandidato> respuestaRepo,
        IRepository<ProcesoSeleccion> procesoRepo, IAnalisisIAService iaService, IUnitOfWork uow)
    {
        _resultadoRepo = resultadoRepo;
        _sesionRepo = sesionRepo;
        _respuestaRepo = respuestaRepo;
        _procesoRepo = procesoRepo;
        _iaService = iaService;
        _uow = uow;
    }

    public async Task<ApiResponse<ResultadoDto>> AnalizarAsync(Guid sesionId)
    {
        var sesion = await _sesionRepo.Query()
            .Include(s => s.Candidato)
            .Include(s => s.Evaluacion).ThenInclude(e => e.Tecnologia)
            .Include(s => s.Evaluacion).ThenInclude(e => e.Secciones).ThenInclude(sec => sec.Preguntas)
            .Include(s => s.Respuestas).ThenInclude(r => r.Pregunta)
            .FirstOrDefaultAsync(s => s.Id == sesionId);

        if (sesion is null) return ApiResponse<ResultadoDto>.NotFound("Sesión no encontrada.");
        if (sesion.Estado != EstadoSesion.Finalizada)
            return ApiResponse<ResultadoDto>.BadRequest("La sesión debe estar finalizada para generar resultados.");

        var existente = await _resultadoRepo.Query().FirstOrDefaultAsync(r => r.SesionId == sesionId);

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

        // Recalcular desde las respuestas actuales (no usar ScoreObtenido que puede estar desactualizado)
        var totalPuntosObtenidos = sesion.Respuestas.Sum(r => r.PuntajeObtenido ?? 0);
        var scoreTotal = sesion.ScoreMaximo > 0
            ? Math.Round((decimal)totalPuntosObtenidos / sesion.ScoreMaximo * 100, 2) : 0;

        // Actualizar ScoreObtenido de la sesión para mantener consistencia
        sesion.ScoreObtenido = totalPuntosObtenidos;
        _sesionRepo.Update(sesion);

        // Construir contexto para análisis IA
        var seccionScores = sesion.Evaluacion.Secciones.Select(sec =>
        {
            var pregIds = sec.Preguntas.Select(p => p.Id).ToHashSet();
            var respSec = sesion.Respuestas.Where(r => pregIds.Contains(r.PreguntaId)).ToList();
            var max = sec.Preguntas.Sum(p => p.Puntaje);
            var obt = respSec.Sum(r => r.PuntajeObtenido ?? 0);
            var pct = max > 0 ? Math.Round((decimal)obt / max * 100, 2) : 0;
            return new SeccionScoreInfo(sec.Nombre, obt, max, pct);
        }).ToList();

        var respuestasAbiertas = sesion.Respuestas
            .Where(r => r.Pregunta.Tipo == TipoPregunta.Abierta || r.Pregunta.Tipo == TipoPregunta.Codigo)
            .Select(r => new RespuestaAbiertaInfo(
                r.Pregunta.Texto, r.Respuesta, r.PuntajeObtenido ?? 0, r.Pregunta.Puntaje))
            .ToList();

        var tiempoTotal = sesion.FechaInicio.HasValue && sesion.FechaFin.HasValue
            ? (int)(sesion.FechaFin.Value - sesion.FechaInicio.Value).TotalSeconds : 0;

        var contextoIA = new AnalisisContexto(
            sesion.Candidato.Nombre,
            sesion.Evaluacion.Nombre,
            sesion.Evaluacion.Tecnologia.Nombre,
            sesion.Evaluacion.Nivel.ToString(),
            scoreTotal,
            seccionScores,
            fortalezas,
            brechas,
            respuestasAbiertas,
            sesion.Evaluacion.Secciones.SelectMany(s => s.Preguntas).Count(),
            sesion.Respuestas.Count,
            tiempoTotal
        );

        var recomendacion = await _iaService.GenerarAnalisisAsync(contextoIA);

        ResultadoEvaluacion resultado;
        string mensaje;

        if (existente is not null)
        {
            // Re-análisis: actualizar resultado existente (permite regenerar tras revisión manual)
            existente.ScoreTotal = scoreTotal;
            existente.ScorePorSeccion = JsonSerializer.Serialize(scoresPorSeccion);
            existente.BrechasIdentificadas = brechas.Count > 0 ? string.Join(", ", brechas) : null;
            existente.FortalezasIdentificadas = fortalezas.Count > 0 ? string.Join(", ", fortalezas) : null;
            existente.RecomendacionIA = recomendacion;
            existente.FechaAnalisis = DateTime.UtcNow;
            _resultadoRepo.Update(existente);
            resultado = existente;
            mensaje = "Análisis actualizado exitosamente.";
        }
        else
        {
            resultado = new ResultadoEvaluacion
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
            mensaje = "Análisis generado exitosamente.";
        }

        await _uow.SaveChangesAsync();

        return ApiResponse<ResultadoDto>.Ok(ToDto(resultado, sesion.Candidato.Nombre, sesion.Evaluacion.Nombre), mensaje);
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

    public async Task<ApiResponse<ResultadoDto>> GetMiResultadoAsync(Guid sesionId, Guid candidatoId)
    {
        var resultado = await _resultadoRepo.Query()
            .Include(r => r.Sesion).ThenInclude(s => s.Candidato)
            .Include(r => r.Sesion).ThenInclude(s => s.Evaluacion)
            .FirstOrDefaultAsync(r => r.SesionId == sesionId);

        if (resultado is null) return ApiResponse<ResultadoDto>.NotFound("Resultado no encontrado.");
        if (resultado.Sesion.CandidatoId != candidatoId)
            return ApiResponse<ResultadoDto>.Forbidden("No tienes acceso a este resultado.");

        return ApiResponse<ResultadoDto>.Ok(ToDto(resultado, resultado.Sesion.Candidato.Nombre, resultado.Sesion.Evaluacion.Nombre));
    }

    public async Task<ApiResponse<RespuestaSesionDto>> RevisarRespuestaAsync(Guid sesionId, Guid respuestaId, RevisarRespuestaDto dto)
    {
        var sesion = await _sesionRepo.Query()
            .Include(s => s.Respuestas)
            .FirstOrDefaultAsync(s => s.Id == sesionId);

        if (sesion is null) return ApiResponse<RespuestaSesionDto>.NotFound("Sesión no encontrada.");
        if (sesion.Estado != EstadoSesion.Finalizada)
            return ApiResponse<RespuestaSesionDto>.BadRequest("Solo se pueden revisar respuestas de sesiones finalizadas.");

        var respuesta = await _respuestaRepo.Query()
            .Include(r => r.Pregunta)
            .Include(r => r.OpcionSeleccionada)
            .FirstOrDefaultAsync(r => r.Id == respuestaId && r.SesionId == sesionId);

        if (respuesta is null) return ApiResponse<RespuestaSesionDto>.NotFound("Respuesta no encontrada en esta sesión.");

        if (respuesta.Pregunta.Tipo != TipoPregunta.Abierta && respuesta.Pregunta.Tipo != TipoPregunta.Codigo)
            return ApiResponse<RespuestaSesionDto>.BadRequest("Solo se pueden revisar manualmente preguntas de tipo Abierta o Código.");

        if (dto.PuntajeObtenido < 0 || dto.PuntajeObtenido > respuesta.Pregunta.Puntaje)
            return ApiResponse<RespuestaSesionDto>.BadRequest($"El puntaje debe estar entre 0 y {respuesta.Pregunta.Puntaje}.");

        respuesta.PuntajeObtenido = dto.PuntajeObtenido;
        respuesta.EsCorrecta = dto.PuntajeObtenido > 0;
        respuesta.ComentarioRevisor = dto.Comentario;
        _respuestaRepo.Update(respuesta);

        // Recalcular ScoreObtenido de la sesión en memoria
        var otrasRespuestas = sesion.Respuestas
            .Where(r => r.Id != respuestaId)
            .Sum(r => r.PuntajeObtenido ?? 0);
        sesion.ScoreObtenido = otrasRespuestas + dto.PuntajeObtenido;
        _sesionRepo.Update(sesion);

        await _uow.SaveChangesAsync();

        return ApiResponse<RespuestaSesionDto>.Ok(new RespuestaSesionDto(
            respuesta.Id, respuesta.PreguntaId, respuesta.Pregunta.Texto,
            (int)respuesta.Pregunta.Tipo, respuesta.Pregunta.Tipo.ToString(),
            respuesta.Respuesta, respuesta.TiempoRespuestaSegundos,
            respuesta.EsCorrecta, respuesta.PuntajeObtenido, respuesta.Pregunta.Puntaje,
            respuesta.OpcionSeleccionadaId, respuesta.OpcionSeleccionada?.Texto,
            respuesta.ComentarioRevisor,
            respuesta.CreatedAt), "Respuesta revisada correctamente.");
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

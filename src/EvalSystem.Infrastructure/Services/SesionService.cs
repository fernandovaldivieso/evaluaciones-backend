using EvalSystem.Application.Common;
using EvalSystem.Application.DTOs.Sesiones;
using EvalSystem.Application.Interfaces;
using EvalSystem.Domain.Entities;
using EvalSystem.Domain.Enums;
using EvalSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EvalSystem.Infrastructure.Services;

public class SesionService : ISesionService
{
    private readonly IRepository<SesionEvaluacion> _sesionRepo;
    private readonly IRepository<RespuestaCandidato> _respuestaRepo;
    private readonly IRepository<Evaluacion> _evalRepo;
    private readonly IRepository<Pregunta> _preguntaRepo;
    private readonly IRepository<OpcionRespuesta> _opcionRepo;
    private readonly IRepository<ProcesoCandidato> _pcRepo;
    private readonly IUnitOfWork _uow;

    public SesionService(IRepository<SesionEvaluacion> sesionRepo, IRepository<RespuestaCandidato> respuestaRepo,
        IRepository<Evaluacion> evalRepo, IRepository<Pregunta> preguntaRepo,
        IRepository<OpcionRespuesta> opcionRepo, IRepository<ProcesoCandidato> pcRepo, IUnitOfWork uow)
    {
        _sesionRepo = sesionRepo;
        _respuestaRepo = respuestaRepo;
        _evalRepo = evalRepo;
        _preguntaRepo = preguntaRepo;
        _opcionRepo = opcionRepo;
        _pcRepo = pcRepo;
        _uow = uow;
    }

    public async Task<ApiResponse<SesionDto>> IniciarAsync(IniciarSesionDto dto, Guid candidatoId)
    {
        var evaluacion = await _evalRepo.Query()
            .Include(e => e.Secciones).ThenInclude(s => s.Preguntas)
            .FirstOrDefaultAsync(e => e.Id == dto.EvaluacionId);

        if (evaluacion is null)
            return ApiResponse<SesionDto>.NotFound("Evaluación no encontrada.");

        if (!evaluacion.Activa)
            return ApiResponse<SesionDto>.BadRequest("La evaluación no está activa.");

        // Resolve procesoId: use the one from request or auto-detect from enrollment
        var resolvedProcesoId = dto.ProcesoId;
        if (!resolvedProcesoId.HasValue)
        {
            resolvedProcesoId = await _pcRepo.Query()
                .Where(pc => pc.CandidatoId == candidatoId
                    && (pc.Proceso.Estado == EstadoProceso.Abierto || pc.Proceso.Estado == EstadoProceso.EnCurso)
                    && pc.Proceso.Evaluaciones.Any(pe => pe.EvaluacionId == dto.EvaluacionId))
                .Select(pc => (Guid?)pc.ProcesoId)
                .FirstOrDefaultAsync();
        }

        if (!resolvedProcesoId.HasValue)
            return ApiResponse<SesionDto>.Forbidden("No tienes acceso a esta evaluación.");

        // Check for existing active session (Pendiente or EnProgreso) → return it
        var sesionActiva = await _sesionRepo.Query()
            .Include(s => s.Candidato).Include(s => s.Evaluacion)
            .FirstOrDefaultAsync(s => s.CandidatoId == candidatoId && s.EvaluacionId == dto.EvaluacionId
                && (s.Estado == EstadoSesion.Pendiente || s.Estado == EstadoSesion.EnProgreso));

        if (sesionActiva is not null)
            return ApiResponse<SesionDto>.Ok(ToDto(sesionActiva), "Sesión activa existente.");

        // Check for completed session → cannot retake
        var sesionFinalizada = await _sesionRepo.Query()
            .AnyAsync(s => s.CandidatoId == candidatoId && s.EvaluacionId == dto.EvaluacionId
                && s.Estado == EstadoSesion.Finalizada);

        if (sesionFinalizada)
            return ApiResponse<SesionDto>.Conflict("Ya completaste esta evaluación.");

        var scoreMaximo = evaluacion.Secciones.SelectMany(s => s.Preguntas).Sum(p => p.Puntaje);

        var sesion = new SesionEvaluacion
        {
            Estado = EstadoSesion.EnProgreso,
            FechaInicio = DateTime.UtcNow,
            ScoreMaximo = scoreMaximo,
            CandidatoId = candidatoId,
            EvaluacionId = dto.EvaluacionId,
            ProcesoId = resolvedProcesoId
        };

        await _sesionRepo.AddAsync(sesion);
        await _uow.SaveChangesAsync();

        var saved = await _sesionRepo.Query()
            .Include(s => s.Candidato).Include(s => s.Evaluacion)
            .FirstAsync(s => s.Id == sesion.Id);

        return ApiResponse<SesionDto>.Created(ToDto(saved));
    }

    public async Task<ApiResponse<SesionDto>> GetByIdAsync(Guid sesionId)
    {
        var s = await _sesionRepo.Query()
            .Include(x => x.Candidato).Include(x => x.Evaluacion)
            .FirstOrDefaultAsync(x => x.Id == sesionId);

        if (s is null) return ApiResponse<SesionDto>.NotFound("Sesión no encontrada.");
        return ApiResponse<SesionDto>.Ok(ToDto(s));
    }

    public async Task<ApiResponse<RespuestaDto>> ResponderAsync(Guid sesionId, ResponderPreguntaDto dto, Guid requestorId)
    {
        var sesion = await _sesionRepo.Query()
            .Include(s => s.Evaluacion)
            .Include(s => s.Respuestas)
            .FirstOrDefaultAsync(s => s.Id == sesionId);

        if (sesion is null) return ApiResponse<RespuestaDto>.NotFound("Sesión no encontrada.");
        if (sesion.CandidatoId != requestorId) return ApiResponse<RespuestaDto>.Forbidden("No tienes acceso a esta sesión.");
        if (sesion.Estado != EstadoSesion.EnProgreso)
            return ApiResponse<RespuestaDto>.BadRequest("La sesión no está en progreso.");

        // Verificar si el tiempo límite fue superado
        if (sesion.FechaInicio.HasValue && sesion.Evaluacion.TiempoLimiteMinutos > 0)
        {
            var limiteExpiracion = sesion.FechaInicio.Value.AddMinutes(sesion.Evaluacion.TiempoLimiteMinutos);
            if (DateTime.UtcNow > limiteExpiracion)
            {
                sesion.Estado = EstadoSesion.Expirada;
                sesion.FechaFin = limiteExpiracion;
                sesion.ScoreObtenido = sesion.Respuestas.Sum(r => r.PuntajeObtenido ?? 0);
                _sesionRepo.Update(sesion);
                await _uow.SaveChangesAsync();
                return ApiResponse<RespuestaDto>.BadRequest("El tiempo de evaluación ha expirado. La sesión fue marcada como expirada.");
            }
        }

        var pregunta = await _preguntaRepo.Query()
            .Include(p => p.Opciones)
            .FirstOrDefaultAsync(p => p.Id == dto.PreguntaId);

        if (pregunta is null) return ApiResponse<RespuestaDto>.NotFound("Pregunta no encontrada.");

        var yaRespondida = await _respuestaRepo.Query()
            .AnyAsync(r => r.SesionId == sesionId && r.PreguntaId == dto.PreguntaId);
        if (yaRespondida)
            return ApiResponse<RespuestaDto>.Conflict("Esta pregunta ya fue respondida en esta sesión.");

        bool? esCorrecta = null;
        int? puntaje = null;

        if (pregunta.Tipo == TipoPregunta.OpcionMultiple || pregunta.Tipo == TipoPregunta.VerdaderoFalso)
        {
            if (dto.OpcionSeleccionadaId.HasValue)
            {
                var opcion = pregunta.Opciones.FirstOrDefault(o => o.Id == dto.OpcionSeleccionadaId.Value);
                if (opcion is not null)
                {
                    esCorrecta = opcion.EsCorrecta;
                    puntaje = opcion.EsCorrecta ? pregunta.Puntaje : 0;
                }
            }
        }

        var respuesta = new RespuestaCandidato
        {
            Respuesta = dto.Respuesta,
            TiempoRespuestaSegundos = dto.TiempoRespuestaSegundos,
            EsCorrecta = esCorrecta,
            PuntajeObtenido = puntaje,
            SesionId = sesionId,
            PreguntaId = dto.PreguntaId,
            OpcionSeleccionadaId = dto.OpcionSeleccionadaId
        };

        await _respuestaRepo.AddAsync(respuesta);
        await _uow.SaveChangesAsync();

        var saved = await _respuestaRepo.Query()
            .Include(r => r.Pregunta)
            .FirstAsync(r => r.Id == respuesta.Id);

        return ApiResponse<RespuestaDto>.Created(ToRespuestaDto(saved));
    }

    public async Task<ApiResponse<SesionDto>> FinalizarAsync(Guid sesionId, Guid requestorId)
    {
        var sesion = await _sesionRepo.Query()
            .Include(s => s.Candidato).Include(s => s.Evaluacion)
            .Include(s => s.Respuestas)
            .FirstOrDefaultAsync(s => s.Id == sesionId);

        if (sesion is null) return ApiResponse<SesionDto>.NotFound("Sesión no encontrada.");
        if (sesion.CandidatoId != requestorId) return ApiResponse<SesionDto>.Forbidden("No tienes acceso a esta sesión.");
        if (sesion.Estado != EstadoSesion.EnProgreso)
            return ApiResponse<SesionDto>.BadRequest("La sesión no está en progreso.");

        sesion.Estado = EstadoSesion.Finalizada;
        sesion.FechaFin = DateTime.UtcNow;
        sesion.ScoreObtenido = sesion.Respuestas.Sum(r => r.PuntajeObtenido ?? 0);

        _sesionRepo.Update(sesion);
        await _uow.SaveChangesAsync();

        return ApiResponse<SesionDto>.Ok(ToDto(sesion), "Sesión finalizada exitosamente.");
    }

    public async Task<ApiResponse<ProgresoSesionDto>> GetProgresoAsync(Guid sesionId)
    {
        var sesion = await _sesionRepo.Query()
            .Include(s => s.Evaluacion).ThenInclude(e => e.Secciones).ThenInclude(sec => sec.Preguntas)
            .Include(s => s.Respuestas)
            .FirstOrDefaultAsync(s => s.Id == sesionId);

        if (sesion is null) return ApiResponse<ProgresoSesionDto>.NotFound("Sesión no encontrada.");

        var totalPreguntas = sesion.Evaluacion.Secciones.SelectMany(s => s.Preguntas).Count();
        var respondidas = sesion.Respuestas.Count;
        var porcentaje = totalPreguntas > 0 ? Math.Round((decimal)respondidas / totalPreguntas * 100, 2) : 0;

        int? tiempoRestante = null;
        if (sesion.FechaInicio.HasValue && sesion.Evaluacion.TiempoLimiteMinutos > 0)
        {
            var transcurrido = (DateTime.UtcNow - sesion.FechaInicio.Value).TotalSeconds;
            var limite = sesion.Evaluacion.TiempoLimiteMinutos * 60;
            tiempoRestante = Math.Max(0, (int)(limite - transcurrido));
        }

        var dto = new ProgresoSesionDto(sesion.Id, totalPreguntas, respondidas,
            porcentaje, tiempoRestante, sesion.Estado.ToString());

        return ApiResponse<ProgresoSesionDto>.Ok(dto);
    }

    public async Task<ApiResponse<IEnumerable<SesionDto>>> GetByCandidatoAsync(Guid candidatoId)
    {
        var sesiones = await _sesionRepo.Query()
            .Where(s => s.CandidatoId == candidatoId)
            .Include(s => s.Candidato).Include(s => s.Evaluacion)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return ApiResponse<IEnumerable<SesionDto>>.Ok(sesiones.Select(ToDto));
    }

    public async Task<ApiResponse<IEnumerable<RespuestaSesionDto>>> GetRespuestasAsync(Guid sesionId)
    {
        if (!await _sesionRepo.Query().AnyAsync(s => s.Id == sesionId))
            return ApiResponse<IEnumerable<RespuestaSesionDto>>.NotFound("Sesión no encontrada.");

        var respuestas = await _respuestaRepo.Query()
            .Where(r => r.SesionId == sesionId)
            .Include(r => r.Pregunta)
            .Include(r => r.OpcionSeleccionada)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();

        return ApiResponse<IEnumerable<RespuestaSesionDto>>.Ok(respuestas.Select(ToRespuestaSesionDto));
    }

    private static SesionDto ToDto(SesionEvaluacion s) => new(
        s.Id, (int)s.Estado, s.Estado.ToString(), s.FechaInicio, s.FechaFin,
        s.ScoreObtenido, s.ScoreMaximo, s.CandidatoId, s.Candidato.Nombre,
        s.EvaluacionId, s.Evaluacion.Nombre, s.ProcesoId, s.CreatedAt);

    private static RespuestaDto ToRespuestaDto(RespuestaCandidato r) => new(
        r.Id, r.PreguntaId, r.Pregunta.Texto, r.Respuesta,
        r.TiempoRespuestaSegundos, r.EsCorrecta, r.PuntajeObtenido, r.CreatedAt);

    private static RespuestaSesionDto ToRespuestaSesionDto(RespuestaCandidato r) => new(
        r.Id, r.PreguntaId, r.Pregunta.Texto,
        (int)r.Pregunta.Tipo, r.Pregunta.Tipo.ToString(),
        r.Respuesta, r.TiempoRespuestaSegundos,
        r.EsCorrecta, r.PuntajeObtenido, r.Pregunta.Puntaje,
        r.OpcionSeleccionadaId, r.OpcionSeleccionada?.Texto,
        r.ComentarioRevisor,
        r.CreatedAt);
}

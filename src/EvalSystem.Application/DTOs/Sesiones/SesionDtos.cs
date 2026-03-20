namespace EvalSystem.Application.DTOs.Sesiones;

public record SesionDto(Guid Id, int Estado, string EstadoNombre, DateTime? FechaInicio, DateTime? FechaFin,
    int? ScoreObtenido, int ScoreMaximo, Guid CandidatoId, string CandidatoNombre,
    Guid EvaluacionId, string EvaluacionNombre, Guid? ProcesoId, DateTime CreatedAt);

public record IniciarSesionDto(Guid EvaluacionId, Guid? ProcesoId);

public record ResponderPreguntaDto(Guid PreguntaId, string Respuesta, Guid? OpcionSeleccionadaId, int TiempoRespuestaSegundos);

public record ProgresoSesionDto(Guid SesionId, int TotalPreguntas, int PreguntasRespondidas,
    decimal PorcentajeCompletado, int? TiempoRestanteSegundos, string Estado);

public record RespuestaDto(Guid Id, Guid PreguntaId, string PreguntaTexto, string Respuesta,
    int TiempoRespuestaSegundos, bool? EsCorrecta, int? PuntajeObtenido, DateTime CreatedAt);

// Vista detallada de respuesta para revisión del reclutador
public record RespuestaSesionDto(Guid Id, Guid PreguntaId, string PreguntaTexto,
    int TipoPregunta, string TipoPreguntaNombre, string Respuesta, int TiempoRespuestaSegundos,
    bool? EsCorrecta, int? PuntajeObtenido, int PuntajeMaximo,
    Guid? OpcionSeleccionadaId, string? OpcionSeleccionadaTexto,
    string? ComentarioRevisor, DateTime CreatedAt);

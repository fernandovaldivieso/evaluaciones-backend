namespace EvalSystem.Application.DTOs.Procesos;

public record ProcesoDto(Guid Id, string Nombre, string? Descripcion, string? Puesto,
    int Estado, string EstadoNombre, DateTime? FechaLimite, Guid CreadorId, string CreadorNombre,
    int TotalCandidatos, int TotalEvaluaciones, DateTime CreatedAt);

public record ProcesoDetalleDto(Guid Id, string Nombre, string? Descripcion, string? Puesto,
    int Estado, string EstadoNombre, DateTime? FechaLimite, Guid CreadorId, string CreadorNombre,
    List<CandidatoProcesoDto> Candidatos, List<EvaluacionProcesoDto> Evaluaciones, DateTime CreatedAt);

public record CandidatoProcesoDto(Guid Id, string Nombre, string Email);
public record EvaluacionProcesoDto(Guid Id, string Nombre, string TecnologiaNombre, string NivelNombre);

public record CreateProcesoDto(string Nombre, string? Descripcion, string? Puesto, DateTime? FechaLimite);
public record UpdateProcesoDto(string? Nombre, string? Descripcion, string? Puesto, int? Estado, DateTime? FechaLimite);
public record AsignarCandidatosDto(List<Guid> CandidatoIds);
public record AsignarEvaluacionesDto(List<Guid> EvaluacionIds);

// Vista de sesiones dentro de un proceso (recruiter dashboard)
public record SesionProcesoDto(Guid SesionId, Guid CandidatoId, string CandidatoNombre, string CandidatoEmail,
    Guid EvaluacionId, string EvaluacionNombre, string TecnologiaNombre,
    int Estado, string EstadoNombre, DateTime? FechaInicio, DateTime? FechaFin,
    int? ScoreObtenido, int ScoreMaximo, decimal? ScorePorcentaje,
    bool TieneResultado, DateTime CreatedAt);

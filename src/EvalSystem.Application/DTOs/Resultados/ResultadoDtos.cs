namespace EvalSystem.Application.DTOs.Resultados;

public record ResultadoDto(Guid Id, decimal ScoreTotal, string? ScorePorSeccion, string? BrechasIdentificadas,
    string? FortalezasIdentificadas, string? RecomendacionIA, DateTime FechaAnalisis,
    Guid SesionId, string CandidatoNombre, string EvaluacionNombre);

public record ComparacionCandidatoDto(Guid CandidatoId, string CandidatoNombre,
    decimal ScoreTotal, string? Fortalezas, string? Brechas, int Posicion);

public record RankingProcesoDto(Guid ProcesoId, string ProcesoNombre,
    List<ComparacionCandidatoDto> Ranking);

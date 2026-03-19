namespace EvalSystem.Application.DTOs.Evaluaciones;

public record SeccionDto(Guid Id, string Nombre, string? Descripcion, int Orden, Guid EvaluacionId, DateTime CreatedAt);
public record CreateSeccionDto(string Nombre, string? Descripcion, int Orden);
public record UpdateSeccionDto(string? Nombre, string? Descripcion, int? Orden);

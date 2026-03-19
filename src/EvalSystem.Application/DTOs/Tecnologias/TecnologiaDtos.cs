namespace EvalSystem.Application.DTOs.Tecnologias;

public record TecnologiaDto(Guid Id, string Nombre, string? Descripcion, bool Activa, DateTime CreatedAt);
public record CreateTecnologiaDto(string Nombre, string? Descripcion);
public record UpdateTecnologiaDto(string? Nombre, string? Descripcion, bool? Activa);

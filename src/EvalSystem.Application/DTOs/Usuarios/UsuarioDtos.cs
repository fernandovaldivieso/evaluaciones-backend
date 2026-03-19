namespace EvalSystem.Application.DTOs.Usuarios;

public record UsuarioDto(Guid Id, string Nombre, string Email, string Rol, bool Activo, DateTime CreatedAt);
public record CreateUsuarioDto(string Nombre, string Email, string Password, int Rol);
public record UpdateUsuarioDto(string? Nombre, string? Email, int? Rol, bool? Activo);

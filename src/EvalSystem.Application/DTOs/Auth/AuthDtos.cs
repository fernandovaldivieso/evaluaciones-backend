namespace EvalSystem.Application.DTOs.Auth;

public record LoginRequest(string Email, string Password);
public record RegisterRequest(string Nombre, string Email, string Password, int Rol);
public record RefreshTokenRequest(string Token);
public record AuthResponse(string Token, string RefreshToken, DateTime Expira, UsuarioInfo Usuario);
public record UsuarioInfo(Guid Id, string Nombre, string Email, string Rol);

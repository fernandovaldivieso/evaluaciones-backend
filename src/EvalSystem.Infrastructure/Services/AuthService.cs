using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EvalSystem.Application.Common;
using EvalSystem.Application.DTOs.Auth;
using EvalSystem.Application.Interfaces;
using EvalSystem.Domain.Entities;
using EvalSystem.Domain.Enums;
using EvalSystem.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace EvalSystem.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IRepository<Usuario> _userRepo;
    private readonly IRepository<RefreshToken> _tokenRepo;
    private readonly IUnitOfWork _uow;
    private readonly IConfiguration _config;

    public AuthService(IRepository<Usuario> userRepo, IRepository<RefreshToken> tokenRepo,
        IUnitOfWork uow, IConfiguration config)
    {
        _userRepo = userRepo;
        _tokenRepo = tokenRepo;
        _uow = uow;
        _config = config;
    }

    public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        var existing = await _userRepo.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existing is not null)
            return ApiResponse<AuthResponse>.Conflict($"Ya existe un usuario con el email '{request.Email}'.");

        if (!Enum.IsDefined(typeof(RolUsuario), request.Rol))
            return ApiResponse<AuthResponse>.BadRequest("Rol no válido.");

        var usuario = new Usuario
        {
            Nombre = request.Nombre,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Rol = (RolUsuario)request.Rol
        };

        await _userRepo.AddAsync(usuario);
        await _uow.SaveChangesAsync();

        var auth = await GenerateTokens(usuario);
        return ApiResponse<AuthResponse>.Created(auth);
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var usuario = await _userRepo.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (usuario is null || !BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
            return ApiResponse<AuthResponse>.Unauthorized("Credenciales inválidas.");

        if (!usuario.Activo)
            return ApiResponse<AuthResponse>.Forbidden("La cuenta está desactivada.");

        var auth = await GenerateTokens(usuario);
        return ApiResponse<AuthResponse>.Ok(auth, "Login exitoso.");
    }

    public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var stored = await _tokenRepo.FirstOrDefaultAsync(t => t.Token == request.Token && !t.Revocado);
        if (stored is null || stored.Expira < DateTime.UtcNow)
            return ApiResponse<AuthResponse>.Unauthorized("Token de refresco inválido o expirado.");

        stored.Revocado = true;
        _tokenRepo.Update(stored);

        var usuario = await _userRepo.GetByIdAsync(stored.UsuarioId);
        if (usuario is null)
            return ApiResponse<AuthResponse>.NotFound("Usuario no encontrado.");

        var auth = await GenerateTokens(usuario);
        return ApiResponse<AuthResponse>.Ok(auth);
    }

    public async Task<ApiResponse> LogoutAsync(string refreshToken)
    {
        var stored = await _tokenRepo.FirstOrDefaultAsync(t => t.Token == refreshToken && !t.Revocado);
        if (stored is not null)
        {
            stored.Revocado = true;
            _tokenRepo.Update(stored);
            await _uow.SaveChangesAsync();
        }
        return ApiResponse.Ok("Sesión cerrada.");
    }

    private async Task<AuthResponse> GenerateTokens(Usuario usuario)
    {
        var jwtKey = _config["Jwt:Key"]!;
        var jwtIssuer = _config["Jwt:Issuer"]!;
        var jwtAudience = _config["Jwt:Audience"]!;
        var expireMinutes = int.Parse(_config["Jwt:ExpireMinutes"] ?? "60");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Name, usuario.Nombre),
            new Claim(ClaimTypes.Role, usuario.Rol.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expira = DateTime.UtcNow.AddMinutes(expireMinutes);

        var token = new JwtSecurityToken(jwtIssuer, jwtAudience, claims,
            expires: expira, signingCredentials: credentials);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            Expira = DateTime.UtcNow.AddDays(7),
            UsuarioId = usuario.Id
        };

        await _tokenRepo.AddAsync(refreshToken);
        await _uow.SaveChangesAsync();

        return new AuthResponse(jwt, refreshToken.Token, expira,
            new UsuarioInfo(usuario.Id, usuario.Nombre, usuario.Email, usuario.Rol.ToString()));
    }
}

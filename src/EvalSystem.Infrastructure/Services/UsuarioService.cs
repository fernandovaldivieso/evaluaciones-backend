using EvalSystem.Application.Common;
using EvalSystem.Application.DTOs.Usuarios;
using EvalSystem.Application.Interfaces;
using EvalSystem.Domain.Entities;
using EvalSystem.Domain.Enums;
using EvalSystem.Domain.Interfaces;

namespace EvalSystem.Infrastructure.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IRepository<Usuario> _repo;
    private readonly IUnitOfWork _uow;

    public UsuarioService(IRepository<Usuario> repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<ApiResponse<IEnumerable<UsuarioDto>>> GetAllAsync()
    {
        var usuarios = await _repo.GetAllAsync();
        var dtos = usuarios.Select(u => ToDto(u));
        return ApiResponse<IEnumerable<UsuarioDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<UsuarioDto>> GetByIdAsync(Guid id)
    {
        var u = await _repo.GetByIdAsync(id);
        if (u is null) return ApiResponse<UsuarioDto>.NotFound($"Usuario con Id '{id}' no encontrado.");
        return ApiResponse<UsuarioDto>.Ok(ToDto(u));
    }

    public async Task<ApiResponse<IEnumerable<UsuarioDto>>> GetCandidatosAsync()
    {
        var candidatos = await _repo.FindAsync(u => u.Rol == RolUsuario.Candidato && u.Activo);
        return ApiResponse<IEnumerable<UsuarioDto>>.Ok(candidatos.Select(u => ToDto(u)));
    }

    public async Task<ApiResponse<UsuarioDto>> CreateAsync(CreateUsuarioDto dto)
    {
        var existing = await _repo.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (existing is not null)
            return ApiResponse<UsuarioDto>.Conflict($"Ya existe un usuario con email '{dto.Email}'.");

        var usuario = new Usuario
        {
            Nombre = dto.Nombre,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Rol = (RolUsuario)dto.Rol
        };

        await _repo.AddAsync(usuario);
        await _uow.SaveChangesAsync();
        return ApiResponse<UsuarioDto>.Created(ToDto(usuario));
    }

    public async Task<ApiResponse<UsuarioDto>> UpdateAsync(Guid id, UpdateUsuarioDto dto)
    {
        var u = await _repo.GetByIdAsync(id);
        if (u is null) return ApiResponse<UsuarioDto>.NotFound($"Usuario con Id '{id}' no encontrado.");

        if (dto.Nombre is not null) u.Nombre = dto.Nombre;
        if (dto.Email is not null)
        {
            var dup = await _repo.FirstOrDefaultAsync(x => x.Email == dto.Email && x.Id != id);
            if (dup is not null) return ApiResponse<UsuarioDto>.Conflict($"Email '{dto.Email}' ya está en uso.");
            u.Email = dto.Email;
        }
        if (dto.Rol.HasValue) u.Rol = (RolUsuario)dto.Rol.Value;
        if (dto.Activo.HasValue) u.Activo = dto.Activo.Value;

        _repo.Update(u);
        await _uow.SaveChangesAsync();
        return ApiResponse<UsuarioDto>.Ok(ToDto(u));
    }

    public async Task<ApiResponse> DeleteAsync(Guid id)
    {
        var u = await _repo.GetByIdAsync(id);
        if (u is null) return ApiResponse.NotFound($"Usuario con Id '{id}' no encontrado.");
        _repo.SoftDelete(u);
        await _uow.SaveChangesAsync();
        return ApiResponse.Ok("Usuario eliminado correctamente.");
    }

    private static UsuarioDto ToDto(Usuario u)
        => new(u.Id, u.Nombre, u.Email, u.Rol.ToString(), u.Activo, u.CreatedAt);
}

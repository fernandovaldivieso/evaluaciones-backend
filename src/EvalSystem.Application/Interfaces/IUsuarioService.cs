using EvalSystem.Application.Common;
using EvalSystem.Application.DTOs.Usuarios;

namespace EvalSystem.Application.Interfaces;

public interface IUsuarioService
{
    Task<ApiResponse<IEnumerable<UsuarioDto>>> GetAllAsync();
    Task<ApiResponse<UsuarioDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<UsuarioDto>> CreateAsync(CreateUsuarioDto dto);
    Task<ApiResponse<UsuarioDto>> UpdateAsync(Guid id, UpdateUsuarioDto dto);
    Task<ApiResponse> DeleteAsync(Guid id);
}

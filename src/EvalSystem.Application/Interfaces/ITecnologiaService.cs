using EvalSystem.Application.Common;
using EvalSystem.Application.DTOs.Tecnologias;

namespace EvalSystem.Application.Interfaces;

public interface ITecnologiaService
{
    Task<ApiResponse<IEnumerable<TecnologiaDto>>> GetAllAsync();
    Task<ApiResponse<TecnologiaDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<TecnologiaDto>> CreateAsync(CreateTecnologiaDto dto);
    Task<ApiResponse<TecnologiaDto>> UpdateAsync(Guid id, UpdateTecnologiaDto dto);
}

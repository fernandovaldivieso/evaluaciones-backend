using EvalSystem.Application.Common;
using EvalSystem.Application.DTOs.Evaluaciones;
using EvalSystem.Application.DTOs.Procesos;

namespace EvalSystem.Application.Interfaces;

public interface IProcesoService
{
    Task<ApiResponse<IEnumerable<ProcesoDto>>> GetAllAsync();
    Task<ApiResponse<ProcesoDetalleDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<ProcesoDto>> CreateAsync(CreateProcesoDto dto, Guid creadorId);
    Task<ApiResponse<ProcesoDto>> UpdateAsync(Guid id, UpdateProcesoDto dto);
    Task<ApiResponse> AsignarCandidatosAsync(Guid procesoId, AsignarCandidatosDto dto);
    Task<ApiResponse> AsignarEvaluacionesAsync(Guid procesoId, AsignarEvaluacionesDto dto);
    Task<ApiResponse<IEnumerable<EvaluacionDto>>> GetMisEvaluacionesAsync(Guid candidatoId);
}

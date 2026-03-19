using EvalSystem.Application.Common;
using EvalSystem.Application.DTOs.Resultados;

namespace EvalSystem.Application.Interfaces;

public interface IResultadoService
{
    Task<ApiResponse<ResultadoDto>> AnalizarAsync(Guid sesionId);
    Task<ApiResponse<ResultadoDto>> GetBySesionAsync(Guid sesionId);
    Task<ApiResponse<RankingProcesoDto>> GetRankingAsync(Guid procesoId);
}

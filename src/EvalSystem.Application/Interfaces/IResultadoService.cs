using EvalSystem.Application.Common;
using EvalSystem.Application.DTOs.Resultados;
using EvalSystem.Application.DTOs.Sesiones;

namespace EvalSystem.Application.Interfaces;

public interface IResultadoService
{
    Task<ApiResponse<ResultadoDto>> AnalizarAsync(Guid sesionId);
    Task<ApiResponse<ResultadoDto>> GetBySesionAsync(Guid sesionId);
    Task<ApiResponse<ResultadoDto>> GetMiResultadoAsync(Guid sesionId, Guid candidatoId);
    Task<ApiResponse<RankingProcesoDto>> GetRankingAsync(Guid procesoId);
    Task<ApiResponse<RespuestaSesionDto>> RevisarRespuestaAsync(Guid sesionId, Guid respuestaId, RevisarRespuestaDto dto);
}

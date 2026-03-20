using EvalSystem.Application.Common;
using EvalSystem.Application.DTOs.Sesiones;

namespace EvalSystem.Application.Interfaces;

public interface ISesionService
{
    Task<ApiResponse<SesionDto>> IniciarAsync(IniciarSesionDto dto, Guid candidatoId);
    Task<ApiResponse<SesionDto>> GetByIdAsync(Guid sesionId);
    Task<ApiResponse<RespuestaDto>> ResponderAsync(Guid sesionId, ResponderPreguntaDto dto, Guid requestorId);
    Task<ApiResponse<SesionDto>> FinalizarAsync(Guid sesionId, Guid requestorId);
    Task<ApiResponse<ProgresoSesionDto>> GetProgresoAsync(Guid sesionId);
    Task<ApiResponse<IEnumerable<SesionDto>>> GetByCandidatoAsync(Guid candidatoId);
    Task<ApiResponse<IEnumerable<RespuestaSesionDto>>> GetRespuestasAsync(Guid sesionId);
}

using EvalSystem.Application.Common;
using EvalSystem.Application.DTOs.Evaluaciones;

namespace EvalSystem.Application.Interfaces;

public interface IEvaluacionService
{
    Task<ApiResponse<IEnumerable<EvaluacionDto>>> GetAllAsync();
    Task<ApiResponse<EvaluacionDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<EvaluacionDetalleDto>> GetDetalleAsync(Guid id);
    Task<ApiResponse<EvaluacionDto>> CreateAsync(CreateEvaluacionDto dto);
    Task<ApiResponse<EvaluacionDto>> UpdateAsync(Guid id, UpdateEvaluacionDto dto);
    Task<ApiResponse> DeleteAsync(Guid id);
    Task<ApiResponse<EvaluacionDto>> DuplicarAsync(Guid id);

    // Secciones
    Task<ApiResponse<IEnumerable<SeccionDto>>> GetSeccionesAsync(Guid evaluacionId);
    Task<ApiResponse<SeccionDto>> CreateSeccionAsync(Guid evaluacionId, CreateSeccionDto dto);
    Task<ApiResponse<SeccionDto>> UpdateSeccionAsync(Guid seccionId, UpdateSeccionDto dto);
    Task<ApiResponse> DeleteSeccionAsync(Guid seccionId);

    // Preguntas
    Task<ApiResponse<IEnumerable<PreguntaDto>>> GetPreguntasAsync(Guid seccionId);
    Task<ApiResponse<PreguntaDto>> CreatePreguntaAsync(Guid seccionId, CreatePreguntaDto dto);
    Task<ApiResponse<PreguntaDto>> UpdatePreguntaAsync(Guid preguntaId, UpdatePreguntaDto dto);
    Task<ApiResponse> DeletePreguntaAsync(Guid preguntaId);
}

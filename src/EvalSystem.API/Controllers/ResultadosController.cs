using EvalSystem.Application.DTOs.Resultados;
using EvalSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EvalSystem.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class ResultadosController : BaseApiController
{
    private readonly IResultadoService _service;
    public ResultadosController(IResultadoService service) => _service = service;

    // Solo Admin/Evaluador pueden generar/ver análisis de cualquier sesión
    [HttpPost("analizar/{sesionId:guid}")]
    [Authorize(Roles = "Admin,Evaluador")]
    public async Task<IActionResult> Analizar(Guid sesionId) => Respond(await _service.AnalizarAsync(sesionId));

    [HttpGet("sesion/{sesionId:guid}")]
    [Authorize(Roles = "Admin,Evaluador")]
    public async Task<IActionResult> GetBySesion(Guid sesionId) => Respond(await _service.GetBySesionAsync(sesionId));

    // Candidato puede ver su propio resultado
    [HttpGet("mi-resultado/{sesionId:guid}")]
    public async Task<IActionResult> GetMiResultado(Guid sesionId)
        => Respond(await _service.GetMiResultadoAsync(sesionId, GetUserId()));

    [HttpGet("ranking/{procesoId:guid}")]
    [Authorize(Roles = "Admin,Evaluador")]
    public async Task<IActionResult> GetRanking(Guid procesoId) => Respond(await _service.GetRankingAsync(procesoId));

    // Revision manual de respuestas Abierta/Codigo (solo Admin/Evaluador)
    [HttpPatch("{sesionId:guid}/respuestas/{respuestaId:guid}/revisar")]
    [Authorize(Roles = "Admin,Evaluador")]
    public async Task<IActionResult> RevisarRespuesta(Guid sesionId, Guid respuestaId, [FromBody] RevisarRespuestaDto dto)
        => Respond(await _service.RevisarRespuestaAsync(sesionId, respuestaId, dto));
}

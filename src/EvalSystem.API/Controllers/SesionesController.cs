using EvalSystem.Application.DTOs.Sesiones;
using EvalSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EvalSystem.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class SesionesController : BaseApiController
{
    private readonly ISesionService _service;
    public SesionesController(ISesionService service) => _service = service;

    [HttpPost("iniciar")]
    public async Task<IActionResult> Iniciar([FromBody] IniciarSesionDto dto)
        => Respond(await _service.IniciarAsync(dto, GetUserId()));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) => Respond(await _service.GetByIdAsync(id));

    [HttpPost("{sesionId:guid}/responder")]
    public async Task<IActionResult> Responder(Guid sesionId, [FromBody] ResponderPreguntaDto dto)
        => Respond(await _service.ResponderAsync(sesionId, dto));

    [HttpPost("{sesionId:guid}/finalizar")]
    public async Task<IActionResult> Finalizar(Guid sesionId)
        => Respond(await _service.FinalizarAsync(sesionId));

    [HttpGet("{sesionId:guid}/progreso")]
    public async Task<IActionResult> GetProgreso(Guid sesionId) => Respond(await _service.GetProgresoAsync(sesionId));

    [HttpGet("candidato/{candidatoId:guid}")]
    [Authorize(Roles = "Admin,Evaluador")]
    public async Task<IActionResult> GetByCandidato(Guid candidatoId)
        => Respond(await _service.GetByCandidatoAsync(candidatoId));

    [HttpGet("mis-sesiones")]
    public async Task<IActionResult> GetMisSesiones()
        => Respond(await _service.GetByCandidatoAsync(GetUserId()));
}

using EvalSystem.Application.DTOs.Procesos;
using EvalSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EvalSystem.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class ProcesosController : BaseApiController
{
    private readonly IProcesoService _service;
    public ProcesosController(IProcesoService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Respond(await _service.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) => Respond(await _service.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = "Admin,Evaluador")]
    public async Task<IActionResult> Create([FromBody] CreateProcesoDto dto)
        => Respond(await _service.CreateAsync(dto, GetUserId()));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Evaluador")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProcesoDto dto)
        => Respond(await _service.UpdateAsync(id, dto));

    [HttpPost("{procesoId:guid}/candidatos")]
    [Authorize(Roles = "Admin,Evaluador")]
    public async Task<IActionResult> AsignarCandidatos(Guid procesoId, [FromBody] AsignarCandidatosDto dto)
        => Respond(await _service.AsignarCandidatosAsync(procesoId, dto));

    [HttpPost("{procesoId:guid}/evaluaciones")]
    [Authorize(Roles = "Admin,Evaluador")]
    public async Task<IActionResult> AsignarEvaluaciones(Guid procesoId, [FromBody] AsignarEvaluacionesDto dto)
        => Respond(await _service.AsignarEvaluacionesAsync(procesoId, dto));

    [HttpGet("mis-evaluaciones")]
    [Authorize(Roles = "Candidato")]
    public async Task<IActionResult> MisEvaluaciones()
        => Respond(await _service.GetMisEvaluacionesAsync(GetUserId()));
}

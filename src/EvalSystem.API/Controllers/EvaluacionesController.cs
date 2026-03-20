using EvalSystem.Application.DTOs.Evaluaciones;
using EvalSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EvalSystem.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class EvaluacionesController : BaseApiController
{
    private readonly IEvaluacionService _service;
    public EvaluacionesController(IEvaluacionService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Respond(await _service.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) => Respond(await _service.GetByIdAsync(id));

    [HttpGet("{id:guid}/detalle")]
    [Authorize(Roles = "Admin,Evaluador")]
    public async Task<IActionResult> GetDetalle(Guid id) => Respond(await _service.GetDetalleAsync(id));

    // Vista segura para candidatos: sin respuestas correctas ni explicaciones
    [HttpGet("{id:guid}/para-candidato")]
    public async Task<IActionResult> GetParaCandidato(Guid id)
        => Respond(await _service.GetParaCandidatoAsync(id));

    [HttpPost]
    [Authorize(Roles = "Admin,Evaluador")]
    public async Task<IActionResult> Create([FromBody] CreateEvaluacionDto dto) => Respond(await _service.CreateAsync(dto));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Evaluador")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEvaluacionDto dto) => Respond(await _service.UpdateAsync(id, dto));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Evaluador")]
    public async Task<IActionResult> Delete(Guid id) => Respond(await _service.DeleteAsync(id));

    [HttpPost("{id:guid}/duplicar")]
    [Authorize(Roles = "Admin,Evaluador")]
    public async Task<IActionResult> Duplicar(Guid id) => Respond(await _service.DuplicarAsync(id));

    // ── Secciones ──────────────────────────────────────

    [HttpGet("{evaluacionId:guid}/secciones")]
    public async Task<IActionResult> GetSecciones(Guid evaluacionId) => Respond(await _service.GetSeccionesAsync(evaluacionId));

    [HttpPost("{evaluacionId:guid}/secciones")]
    [Authorize(Roles = "Admin,Evaluador")]
    public async Task<IActionResult> CreateSeccion(Guid evaluacionId, [FromBody] CreateSeccionDto dto)
        => Respond(await _service.CreateSeccionAsync(evaluacionId, dto));

    [HttpPut("secciones/{seccionId:guid}")]
    [Authorize(Roles = "Admin,Evaluador")]
    public async Task<IActionResult> UpdateSeccion(Guid seccionId, [FromBody] UpdateSeccionDto dto)
        => Respond(await _service.UpdateSeccionAsync(seccionId, dto));

    [HttpDelete("secciones/{seccionId:guid}")]
    [Authorize(Roles = "Admin,Evaluador")]
    public async Task<IActionResult> DeleteSeccion(Guid seccionId) => Respond(await _service.DeleteSeccionAsync(seccionId));

    // ── Preguntas ──────────────────────────────────────

    [HttpGet("secciones/{seccionId:guid}/preguntas")]
    public async Task<IActionResult> GetPreguntas(Guid seccionId) => Respond(await _service.GetPreguntasAsync(seccionId));

    [HttpPost("secciones/{seccionId:guid}/preguntas")]
    [Authorize(Roles = "Admin,Evaluador")]
    public async Task<IActionResult> CreatePregunta(Guid seccionId, [FromBody] CreatePreguntaDto dto)
        => Respond(await _service.CreatePreguntaAsync(seccionId, dto));

    [HttpPut("preguntas/{preguntaId:guid}")]
    [Authorize(Roles = "Admin,Evaluador")]
    public async Task<IActionResult> UpdatePregunta(Guid preguntaId, [FromBody] UpdatePreguntaDto dto)
        => Respond(await _service.UpdatePreguntaAsync(preguntaId, dto));

    [HttpDelete("preguntas/{preguntaId:guid}")]
    [Authorize(Roles = "Admin,Evaluador")]
    public async Task<IActionResult> DeletePregunta(Guid preguntaId) => Respond(await _service.DeletePreguntaAsync(preguntaId));
}

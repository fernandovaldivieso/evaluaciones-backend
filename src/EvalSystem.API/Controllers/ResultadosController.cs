using EvalSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EvalSystem.API.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = "Admin,Evaluador")]
public class ResultadosController : BaseApiController
{
    private readonly IResultadoService _service;
    public ResultadosController(IResultadoService service) => _service = service;

    [HttpPost("analizar/{sesionId:guid}")]
    public async Task<IActionResult> Analizar(Guid sesionId) => Respond(await _service.AnalizarAsync(sesionId));

    [HttpGet("sesion/{sesionId:guid}")]
    public async Task<IActionResult> GetBySesion(Guid sesionId) => Respond(await _service.GetBySesionAsync(sesionId));

    [HttpGet("ranking/{procesoId:guid}")]
    public async Task<IActionResult> GetRanking(Guid procesoId) => Respond(await _service.GetRankingAsync(procesoId));
}

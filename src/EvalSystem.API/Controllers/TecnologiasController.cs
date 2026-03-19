using EvalSystem.Application.DTOs.Tecnologias;
using EvalSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EvalSystem.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class TecnologiasController : BaseApiController
{
    private readonly ITecnologiaService _service;
    public TecnologiasController(ITecnologiaService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Respond(await _service.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) => Respond(await _service.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = "Admin,Evaluador")]
    public async Task<IActionResult> Create([FromBody] CreateTecnologiaDto dto) => Respond(await _service.CreateAsync(dto));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Evaluador")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTecnologiaDto dto) => Respond(await _service.UpdateAsync(id, dto));
}

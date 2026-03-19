using EvalSystem.Application.DTOs.Usuarios;
using EvalSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EvalSystem.API.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsuariosController : BaseApiController
{
    private readonly IUsuarioService _service;
    public UsuariosController(IUsuarioService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Respond(await _service.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) => Respond(await _service.GetByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUsuarioDto dto) => Respond(await _service.CreateAsync(dto));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUsuarioDto dto) => Respond(await _service.UpdateAsync(id, dto));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) => Respond(await _service.DeleteAsync(id));
}

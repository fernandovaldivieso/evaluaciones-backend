using EvalSystem.Application.Common;
using EvalSystem.Application.DTOs.Tecnologias;
using EvalSystem.Application.Interfaces;
using EvalSystem.Domain.Entities;
using EvalSystem.Domain.Interfaces;

namespace EvalSystem.Infrastructure.Services;

public class TecnologiaService : ITecnologiaService
{
    private readonly IRepository<Tecnologia> _repo;
    private readonly IUnitOfWork _uow;

    public TecnologiaService(IRepository<Tecnologia> repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<ApiResponse<IEnumerable<TecnologiaDto>>> GetAllAsync()
    {
        var items = await _repo.GetAllAsync();
        return ApiResponse<IEnumerable<TecnologiaDto>>.Ok(items.Select(ToDto));
    }

    public async Task<ApiResponse<TecnologiaDto>> GetByIdAsync(Guid id)
    {
        var t = await _repo.GetByIdAsync(id);
        if (t is null) return ApiResponse<TecnologiaDto>.NotFound($"Tecnología con Id '{id}' no encontrada.");
        return ApiResponse<TecnologiaDto>.Ok(ToDto(t));
    }

    public async Task<ApiResponse<TecnologiaDto>> CreateAsync(CreateTecnologiaDto dto)
    {
        var dup = await _repo.FirstOrDefaultAsync(t => t.Nombre == dto.Nombre);
        if (dup is not null) return ApiResponse<TecnologiaDto>.Conflict($"Ya existe la tecnología '{dto.Nombre}'.");

        var entity = new Tecnologia { Nombre = dto.Nombre, Descripcion = dto.Descripcion };
        await _repo.AddAsync(entity);
        await _uow.SaveChangesAsync();
        return ApiResponse<TecnologiaDto>.Created(ToDto(entity));
    }

    public async Task<ApiResponse<TecnologiaDto>> UpdateAsync(Guid id, UpdateTecnologiaDto dto)
    {
        var t = await _repo.GetByIdAsync(id);
        if (t is null) return ApiResponse<TecnologiaDto>.NotFound($"Tecnología con Id '{id}' no encontrada.");

        if (dto.Nombre is not null)
        {
            var dup = await _repo.FirstOrDefaultAsync(x => x.Nombre == dto.Nombre && x.Id != id);
            if (dup is not null) return ApiResponse<TecnologiaDto>.Conflict($"Ya existe la tecnología '{dto.Nombre}'.");
            t.Nombre = dto.Nombre;
        }
        if (dto.Descripcion is not null) t.Descripcion = dto.Descripcion;
        if (dto.Activa.HasValue) t.Activa = dto.Activa.Value;

        _repo.Update(t);
        await _uow.SaveChangesAsync();
        return ApiResponse<TecnologiaDto>.Ok(ToDto(t));
    }

    private static TecnologiaDto ToDto(Tecnologia t)
        => new(t.Id, t.Nombre, t.Descripcion, t.Activa, t.CreatedAt);
}

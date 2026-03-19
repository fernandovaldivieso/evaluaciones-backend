using EvalSystem.Application.Common;
using EvalSystem.Application.DTOs.Evaluaciones;
using EvalSystem.Application.Interfaces;
using EvalSystem.Domain.Entities;
using EvalSystem.Domain.Enums;
using EvalSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EvalSystem.Infrastructure.Services;

public class EvaluacionService : IEvaluacionService
{
    private readonly IRepository<Evaluacion> _evalRepo;
    private readonly IRepository<EvaluacionSeccion> _seccionRepo;
    private readonly IRepository<Pregunta> _preguntaRepo;
    private readonly IRepository<OpcionRespuesta> _opcionRepo;
    private readonly IRepository<Tecnologia> _tecRepo;
    private readonly IUnitOfWork _uow;

    public EvaluacionService(IRepository<Evaluacion> evalRepo, IRepository<EvaluacionSeccion> seccionRepo,
        IRepository<Pregunta> preguntaRepo, IRepository<OpcionRespuesta> opcionRepo,
        IRepository<Tecnologia> tecRepo, IUnitOfWork uow)
    {
        _evalRepo = evalRepo;
        _seccionRepo = seccionRepo;
        _preguntaRepo = preguntaRepo;
        _opcionRepo = opcionRepo;
        _tecRepo = tecRepo;
        _uow = uow;
    }

    public async Task<ApiResponse<IEnumerable<EvaluacionDto>>> GetAllAsync()
    {
        var items = await _evalRepo.Query()
            .Include(e => e.Tecnologia)
            .ToListAsync();
        return ApiResponse<IEnumerable<EvaluacionDto>>.Ok(items.Select(ToDto));
    }

    public async Task<ApiResponse<EvaluacionDto>> GetByIdAsync(Guid id)
    {
        var e = await _evalRepo.Query().Include(x => x.Tecnologia).FirstOrDefaultAsync(x => x.Id == id);
        if (e is null) return ApiResponse<EvaluacionDto>.NotFound($"Evaluación con Id '{id}' no encontrada.");
        return ApiResponse<EvaluacionDto>.Ok(ToDto(e));
    }

    public async Task<ApiResponse<EvaluacionDetalleDto>> GetDetalleAsync(Guid id)
    {
        var e = await _evalRepo.Query()
            .Include(x => x.Tecnologia)
            .Include(x => x.Secciones.OrderBy(s => s.Orden))
                .ThenInclude(s => s.Preguntas.OrderBy(p => p.Orden))
                    .ThenInclude(p => p.Opciones.OrderBy(o => o.Orden))
            .FirstOrDefaultAsync(x => x.Id == id);

        if (e is null) return ApiResponse<EvaluacionDetalleDto>.NotFound($"Evaluación con Id '{id}' no encontrada.");

        var dto = new EvaluacionDetalleDto(e.Id, e.Nombre, e.Descripcion, (int)e.Nivel, e.Nivel.ToString(),
            e.TiempoLimiteMinutos, e.Activa, e.TecnologiaId, e.Tecnologia.Nombre,
            e.Secciones.Select(s => new SeccionDetalleDto(s.Id, s.Nombre, s.Descripcion, s.Orden,
                s.Preguntas.Select(p => new PreguntaDetalleDto(p.Id, p.Texto, (int)p.Tipo, p.Tipo.ToString(),
                    p.Puntaje, p.TiempoSegundos, p.Orden, p.Explicacion,
                    p.Opciones.Select(o => new OpcionDto(o.Id, o.Texto, o.EsCorrecta, o.Orden)).ToList()
                )).ToList()
            )).ToList(), e.CreatedAt);

        return ApiResponse<EvaluacionDetalleDto>.Ok(dto);
    }

    public async Task<ApiResponse<EvaluacionDto>> CreateAsync(CreateEvaluacionDto dto)
    {
        if (!await _tecRepo.ExistsAsync(dto.TecnologiaId))
            return ApiResponse<EvaluacionDto>.UnprocessableEntity(
                "Tecnología no encontrada.", new List<string> { $"No existe Tecnología con Id '{dto.TecnologiaId}'." });

        var entity = new Evaluacion
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Nivel = (NivelDificultad)dto.Nivel,
            TiempoLimiteMinutos = dto.TiempoLimiteMinutos,
            TecnologiaId = dto.TecnologiaId
        };

        await _evalRepo.AddAsync(entity);
        await _uow.SaveChangesAsync();

        var saved = await _evalRepo.Query().Include(e => e.Tecnologia).FirstAsync(e => e.Id == entity.Id);
        return ApiResponse<EvaluacionDto>.Created(ToDto(saved));
    }

    public async Task<ApiResponse<EvaluacionDto>> UpdateAsync(Guid id, UpdateEvaluacionDto dto)
    {
        var e = await _evalRepo.Query().Include(x => x.Tecnologia).FirstOrDefaultAsync(x => x.Id == id);
        if (e is null) return ApiResponse<EvaluacionDto>.NotFound($"Evaluación con Id '{id}' no encontrada.");

        if (dto.Nombre is not null) e.Nombre = dto.Nombre;
        if (dto.Descripcion is not null) e.Descripcion = dto.Descripcion;
        if (dto.Nivel.HasValue) e.Nivel = (NivelDificultad)dto.Nivel.Value;
        if (dto.TiempoLimiteMinutos.HasValue) e.TiempoLimiteMinutos = dto.TiempoLimiteMinutos.Value;
        if (dto.Activa.HasValue) e.Activa = dto.Activa.Value;
        if (dto.TecnologiaId.HasValue)
        {
            if (!await _tecRepo.ExistsAsync(dto.TecnologiaId.Value))
                return ApiResponse<EvaluacionDto>.UnprocessableEntity(
                    "Tecnología no encontrada.", new List<string> { $"No existe Tecnología con Id '{dto.TecnologiaId}'." });
            e.TecnologiaId = dto.TecnologiaId.Value;
        }

        _evalRepo.Update(e);
        await _uow.SaveChangesAsync();
        return ApiResponse<EvaluacionDto>.Ok(ToDto(e));
    }

    public async Task<ApiResponse> DeleteAsync(Guid id)
    {
        var e = await _evalRepo.GetByIdAsync(id);
        if (e is null) return ApiResponse.NotFound($"Evaluación con Id '{id}' no encontrada.");
        _evalRepo.SoftDelete(e);
        await _uow.SaveChangesAsync();
        return ApiResponse.Ok("Evaluación eliminada.");
    }

    public async Task<ApiResponse<EvaluacionDto>> DuplicarAsync(Guid id)
    {
        var original = await _evalRepo.Query()
            .Include(x => x.Tecnologia)
            .Include(x => x.Secciones).ThenInclude(s => s.Preguntas).ThenInclude(p => p.Opciones)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (original is null) return ApiResponse<EvaluacionDto>.NotFound($"Evaluación con Id '{id}' no encontrada.");

        var copia = new Evaluacion
        {
            Nombre = $"{original.Nombre} (copia)",
            Descripcion = original.Descripcion,
            Nivel = original.Nivel,
            TiempoLimiteMinutos = original.TiempoLimiteMinutos,
            TecnologiaId = original.TecnologiaId,
            Secciones = original.Secciones.Select(s => new EvaluacionSeccion
            {
                Nombre = s.Nombre, Descripcion = s.Descripcion, Orden = s.Orden,
                Preguntas = s.Preguntas.Select(p => new Pregunta
                {
                    Texto = p.Texto, Tipo = p.Tipo, Puntaje = p.Puntaje,
                    TiempoSegundos = p.TiempoSegundos, Orden = p.Orden, Explicacion = p.Explicacion,
                    Opciones = p.Opciones.Select(o => new OpcionRespuesta
                    {
                        Texto = o.Texto, EsCorrecta = o.EsCorrecta, Orden = o.Orden
                    }).ToList()
                }).ToList()
            }).ToList()
        };

        await _evalRepo.AddAsync(copia);
        await _uow.SaveChangesAsync();

        var saved = await _evalRepo.Query().Include(e => e.Tecnologia).FirstAsync(e => e.Id == copia.Id);
        return ApiResponse<EvaluacionDto>.Created(ToDto(saved), "Evaluación duplicada exitosamente.");
    }

    // ── Secciones ────────────────────────────────────────────

    public async Task<ApiResponse<IEnumerable<SeccionDto>>> GetSeccionesAsync(Guid evaluacionId)
    {
        if (!await _evalRepo.ExistsAsync(evaluacionId))
            return ApiResponse<IEnumerable<SeccionDto>>.NotFound("Evaluación no encontrada.");

        var secciones = await _seccionRepo.Query()
            .Where(s => s.EvaluacionId == evaluacionId).OrderBy(s => s.Orden).ToListAsync();
        return ApiResponse<IEnumerable<SeccionDto>>.Ok(secciones.Select(s =>
            new SeccionDto(s.Id, s.Nombre, s.Descripcion, s.Orden, s.EvaluacionId, s.CreatedAt)));
    }

    public async Task<ApiResponse<SeccionDto>> CreateSeccionAsync(Guid evaluacionId, CreateSeccionDto dto)
    {
        if (!await _evalRepo.ExistsAsync(evaluacionId))
            return ApiResponse<SeccionDto>.NotFound("Evaluación no encontrada.");

        var entity = new EvaluacionSeccion
        {
            Nombre = dto.Nombre, Descripcion = dto.Descripcion, Orden = dto.Orden,
            EvaluacionId = evaluacionId
        };
        await _seccionRepo.AddAsync(entity);
        await _uow.SaveChangesAsync();
        return ApiResponse<SeccionDto>.Created(new SeccionDto(entity.Id, entity.Nombre, entity.Descripcion, entity.Orden, entity.EvaluacionId, entity.CreatedAt));
    }

    public async Task<ApiResponse<SeccionDto>> UpdateSeccionAsync(Guid seccionId, UpdateSeccionDto dto)
    {
        var s = await _seccionRepo.GetByIdAsync(seccionId);
        if (s is null) return ApiResponse<SeccionDto>.NotFound("Sección no encontrada.");

        if (dto.Nombre is not null) s.Nombre = dto.Nombre;
        if (dto.Descripcion is not null) s.Descripcion = dto.Descripcion;
        if (dto.Orden.HasValue) s.Orden = dto.Orden.Value;

        _seccionRepo.Update(s);
        await _uow.SaveChangesAsync();
        return ApiResponse<SeccionDto>.Ok(new SeccionDto(s.Id, s.Nombre, s.Descripcion, s.Orden, s.EvaluacionId, s.CreatedAt));
    }

    public async Task<ApiResponse> DeleteSeccionAsync(Guid seccionId)
    {
        var s = await _seccionRepo.GetByIdAsync(seccionId);
        if (s is null) return ApiResponse.NotFound("Sección no encontrada.");
        _seccionRepo.SoftDelete(s);
        await _uow.SaveChangesAsync();
        return ApiResponse.Ok("Sección eliminada.");
    }

    // ── Preguntas ────────────────────────────────────────────

    public async Task<ApiResponse<IEnumerable<PreguntaDto>>> GetPreguntasAsync(Guid seccionId)
    {
        if (!await _seccionRepo.ExistsAsync(seccionId))
            return ApiResponse<IEnumerable<PreguntaDto>>.NotFound("Sección no encontrada.");

        var preguntas = await _preguntaRepo.Query()
            .Where(p => p.SeccionId == seccionId).OrderBy(p => p.Orden)
            .Include(p => p.Opciones.OrderBy(o => o.Orden))
            .ToListAsync();

        return ApiResponse<IEnumerable<PreguntaDto>>.Ok(preguntas.Select(ToPreguntaDto));
    }

    public async Task<ApiResponse<PreguntaDto>> CreatePreguntaAsync(Guid seccionId, CreatePreguntaDto dto)
    {
        if (!await _seccionRepo.ExistsAsync(seccionId))
            return ApiResponse<PreguntaDto>.NotFound("Sección no encontrada.");

        var pregunta = new Pregunta
        {
            Texto = dto.Texto, Tipo = (TipoPregunta)dto.Tipo, Puntaje = dto.Puntaje,
            TiempoSegundos = dto.TiempoSegundos, Orden = dto.Orden, Explicacion = dto.Explicacion,
            SeccionId = seccionId
        };

        if (dto.Opciones is not null)
        {
            pregunta.Opciones = dto.Opciones.Select(o => new OpcionRespuesta
            {
                Texto = o.Texto, EsCorrecta = o.EsCorrecta, Orden = o.Orden
            }).ToList();
        }

        await _preguntaRepo.AddAsync(pregunta);
        await _uow.SaveChangesAsync();

        var saved = await _preguntaRepo.Query().Include(p => p.Opciones).FirstAsync(p => p.Id == pregunta.Id);
        return ApiResponse<PreguntaDto>.Created(ToPreguntaDto(saved));
    }

    public async Task<ApiResponse<PreguntaDto>> UpdatePreguntaAsync(Guid preguntaId, UpdatePreguntaDto dto)
    {
        var p = await _preguntaRepo.Query().Include(x => x.Opciones).FirstOrDefaultAsync(x => x.Id == preguntaId);
        if (p is null) return ApiResponse<PreguntaDto>.NotFound("Pregunta no encontrada.");

        if (dto.Texto is not null) p.Texto = dto.Texto;
        if (dto.Tipo.HasValue) p.Tipo = (TipoPregunta)dto.Tipo.Value;
        if (dto.Puntaje.HasValue) p.Puntaje = dto.Puntaje.Value;
        if (dto.TiempoSegundos.HasValue) p.TiempoSegundos = dto.TiempoSegundos.Value;
        if (dto.Orden.HasValue) p.Orden = dto.Orden.Value;
        if (dto.Explicacion is not null) p.Explicacion = dto.Explicacion;

        _preguntaRepo.Update(p);
        await _uow.SaveChangesAsync();
        return ApiResponse<PreguntaDto>.Ok(ToPreguntaDto(p));
    }

    public async Task<ApiResponse> DeletePreguntaAsync(Guid preguntaId)
    {
        var p = await _preguntaRepo.GetByIdAsync(preguntaId);
        if (p is null) return ApiResponse.NotFound("Pregunta no encontrada.");
        _preguntaRepo.SoftDelete(p);
        await _uow.SaveChangesAsync();
        return ApiResponse.Ok("Pregunta eliminada.");
    }

    // ── Mapping ──────────────────────────────────────────────

    private static EvaluacionDto ToDto(Evaluacion e) => new(
        e.Id, e.Nombre, e.Descripcion, (int)e.Nivel, e.Nivel.ToString(),
        e.TiempoLimiteMinutos, e.Activa, e.TecnologiaId, e.Tecnologia.Nombre, e.CreatedAt);

    private static PreguntaDto ToPreguntaDto(Pregunta p) => new(
        p.Id, p.Texto, (int)p.Tipo, p.Tipo.ToString(), p.Puntaje,
        p.TiempoSegundos, p.Orden, p.Explicacion, p.SeccionId,
        p.Opciones.Select(o => new OpcionDto(o.Id, o.Texto, o.EsCorrecta, o.Orden)).ToList(),
        p.CreatedAt);
}

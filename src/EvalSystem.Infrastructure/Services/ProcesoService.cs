using EvalSystem.Application.Common;
using EvalSystem.Application.DTOs.Evaluaciones;
using EvalSystem.Application.DTOs.Procesos;
using EvalSystem.Application.Interfaces;
using EvalSystem.Domain.Entities;
using EvalSystem.Domain.Enums;
using EvalSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EvalSystem.Infrastructure.Services;

public class ProcesoService : IProcesoService
{
    private readonly IRepository<ProcesoSeleccion> _procesoRepo;
    private readonly IRepository<ProcesoCandidato> _pcRepo;
    private readonly IRepository<ProcesoEvaluacion> _peRepo;
    private readonly IRepository<Usuario> _usuarioRepo;
    private readonly IRepository<Evaluacion> _evalRepo;
    private readonly IRepository<SesionEvaluacion> _sesionRepo;
    private readonly IUnitOfWork _uow;

    public ProcesoService(IRepository<ProcesoSeleccion> procesoRepo, IRepository<ProcesoCandidato> pcRepo,
        IRepository<ProcesoEvaluacion> peRepo, IRepository<Usuario> usuarioRepo,
        IRepository<Evaluacion> evalRepo, IRepository<SesionEvaluacion> sesionRepo, IUnitOfWork uow)
    {
        _procesoRepo = procesoRepo;
        _pcRepo = pcRepo;
        _peRepo = peRepo;
        _usuarioRepo = usuarioRepo;
        _evalRepo = evalRepo;
        _sesionRepo = sesionRepo;
        _uow = uow;
    }

    public async Task<ApiResponse<IEnumerable<ProcesoDto>>> GetAllAsync()
    {
        var items = await _procesoRepo.Query()
            .Include(p => p.Creador)
            .Include(p => p.Candidatos)
            .Include(p => p.Evaluaciones)
            .ToListAsync();

        return ApiResponse<IEnumerable<ProcesoDto>>.Ok(items.Select(ToDto));
    }

    public async Task<ApiResponse<ProcesoDetalleDto>> GetByIdAsync(Guid id)
    {
        var p = await _procesoRepo.Query()
            .Include(x => x.Creador)
            .Include(x => x.Candidatos).ThenInclude(c => c.Candidato)
            .Include(x => x.Evaluaciones).ThenInclude(e => e.Evaluacion).ThenInclude(ev => ev.Tecnologia)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (p is null) return ApiResponse<ProcesoDetalleDto>.NotFound("Proceso no encontrado.");

        var dto = new ProcesoDetalleDto(p.Id, p.Nombre, p.Descripcion, p.Puesto,
            (int)p.Estado, p.Estado.ToString(), p.FechaLimite, p.CreadorId, p.Creador.Nombre,
            p.Candidatos.Select(c => new CandidatoProcesoDto(c.CandidatoId, c.Candidato.Nombre, c.Candidato.Email)).ToList(),
            p.Evaluaciones.Select(e => new EvaluacionProcesoDto(e.EvaluacionId, e.Evaluacion.Nombre,
                e.Evaluacion.Tecnologia.Nombre, e.Evaluacion.Nivel.ToString())).ToList(),
            p.CreatedAt);

        return ApiResponse<ProcesoDetalleDto>.Ok(dto);
    }

    public async Task<ApiResponse<ProcesoDto>> CreateAsync(CreateProcesoDto dto, Guid creadorId)
    {
        var entity = new ProcesoSeleccion
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Puesto = dto.Puesto,
            FechaLimite = dto.FechaLimite,
            CreadorId = creadorId
        };

        await _procesoRepo.AddAsync(entity);
        await _uow.SaveChangesAsync();

        var saved = await _procesoRepo.Query()
            .Include(p => p.Creador).Include(p => p.Candidatos).Include(p => p.Evaluaciones)
            .FirstAsync(p => p.Id == entity.Id);

        return ApiResponse<ProcesoDto>.Created(ToDto(saved));
    }

    public async Task<ApiResponse<ProcesoDto>> UpdateAsync(Guid id, UpdateProcesoDto dto)
    {
        var p = await _procesoRepo.Query()
            .Include(x => x.Creador).Include(x => x.Candidatos).Include(x => x.Evaluaciones)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (p is null) return ApiResponse<ProcesoDto>.NotFound("Proceso no encontrado.");

        if (dto.Nombre is not null) p.Nombre = dto.Nombre;
        if (dto.Descripcion is not null) p.Descripcion = dto.Descripcion;
        if (dto.Puesto is not null) p.Puesto = dto.Puesto;
        if (dto.Estado.HasValue) p.Estado = (EstadoProceso)dto.Estado.Value;
        if (dto.FechaLimite.HasValue) p.FechaLimite = dto.FechaLimite.Value;

        _procesoRepo.Update(p);
        await _uow.SaveChangesAsync();
        return ApiResponse<ProcesoDto>.Ok(ToDto(p));
    }

    public async Task<ApiResponse> AsignarCandidatosAsync(Guid procesoId, AsignarCandidatosDto dto)
    {
        if (!await _procesoRepo.ExistsAsync(procesoId))
            return ApiResponse.NotFound("Proceso no encontrado.");

        var existentes = await _pcRepo.Query()
            .Where(pc => pc.ProcesoId == procesoId)
            .Select(pc => pc.CandidatoId).ToListAsync();

        var nuevos = dto.CandidatoIds.Except(existentes).ToList();

        foreach (var candidatoId in nuevos)
        {
            if (!await _usuarioRepo.ExistsAsync(candidatoId))
                return ApiResponse.UnprocessableEntity($"Candidato con Id '{candidatoId}' no encontrado.");

            await _pcRepo.AddAsync(new ProcesoCandidato { ProcesoId = procesoId, CandidatoId = candidatoId });
        }

        await _uow.SaveChangesAsync();
        return ApiResponse.Ok($"{nuevos.Count} candidato(s) asignado(s).");
    }

    public async Task<ApiResponse> AsignarEvaluacionesAsync(Guid procesoId, AsignarEvaluacionesDto dto)
    {
        if (!await _procesoRepo.ExistsAsync(procesoId))
            return ApiResponse.NotFound("Proceso no encontrado.");

        var existentes = await _peRepo.Query()
            .Where(pe => pe.ProcesoId == procesoId)
            .Select(pe => pe.EvaluacionId).ToListAsync();

        var nuevas = dto.EvaluacionIds.Except(existentes).ToList();

        foreach (var evaluacionId in nuevas)
        {
            if (!await _evalRepo.ExistsAsync(evaluacionId))
                return ApiResponse.UnprocessableEntity($"Evaluación con Id '{evaluacionId}' no encontrada.");

            await _peRepo.AddAsync(new ProcesoEvaluacion { ProcesoId = procesoId, EvaluacionId = evaluacionId });
        }

        await _uow.SaveChangesAsync();
        return ApiResponse.Ok($"{nuevas.Count} evaluación(es) asignada(s).");
    }

    public async Task<ApiResponse<IEnumerable<EvaluacionDto>>> GetMisEvaluacionesAsync(Guid candidatoId)
    {
        // Get IDs of active processes where the candidate is enrolled
        var procesoIds = await _pcRepo.Query()
            .Where(pc => pc.CandidatoId == candidatoId
                && (pc.Proceso.Estado == EstadoProceso.Abierto
                    || pc.Proceso.Estado == EstadoProceso.EnCurso))
            .Select(pc => pc.ProcesoId)
            .Distinct()
            .ToListAsync();

        if (!procesoIds.Any())
            return ApiResponse<IEnumerable<EvaluacionDto>>.Ok(Enumerable.Empty<EvaluacionDto>());

        // Get evaluation IDs assigned to those processes
        var evalIds = await _peRepo.Query()
            .Where(pe => procesoIds.Contains(pe.ProcesoId))
            .Select(pe => pe.EvaluacionId)
            .Distinct()
            .ToListAsync();

        // Return full evaluation data for active evaluations only
        var evaluaciones = await _evalRepo.Query()
            .Include(e => e.Tecnologia)
            .Where(e => evalIds.Contains(e.Id) && e.Activa)
            .ToListAsync();

        return ApiResponse<IEnumerable<EvaluacionDto>>.Ok(evaluaciones.Select(e => new EvaluacionDto(
            e.Id, e.Nombre, e.Descripcion, (int)e.Nivel, e.Nivel.ToString(),
            e.TiempoLimiteMinutos, e.Activa, e.TecnologiaId, e.Tecnologia.Nombre, e.CreatedAt)));
    }

    public async Task<ApiResponse<IEnumerable<SesionProcesoDto>>> GetSesionesProcesoAsync(Guid procesoId)
    {
        if (!await _procesoRepo.ExistsAsync(procesoId))
            return ApiResponse<IEnumerable<SesionProcesoDto>>.NotFound("Proceso no encontrado.");

        var sesiones = await _sesionRepo.Query()
            .Where(s => s.ProcesoId == procesoId)
            .Include(s => s.Candidato)
            .Include(s => s.Evaluacion).ThenInclude(e => e.Tecnologia)
            .Include(s => s.Resultado)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        var dtos = sesiones.Select(s =>
        {
            var scorePct = s.ScoreMaximo > 0 && s.ScoreObtenido.HasValue
                ? Math.Round((decimal)s.ScoreObtenido.Value / s.ScoreMaximo * 100, 2)
                : (decimal?)null;

            return new SesionProcesoDto(
                s.Id, s.CandidatoId, s.Candidato.Nombre, s.Candidato.Email,
                s.EvaluacionId, s.Evaluacion.Nombre, s.Evaluacion.Tecnologia.Nombre,
                (int)s.Estado, s.Estado.ToString(), s.FechaInicio, s.FechaFin,
                s.ScoreObtenido, s.ScoreMaximo, scorePct,
                s.Resultado is not null, s.CreatedAt);
        });

        return ApiResponse<IEnumerable<SesionProcesoDto>>.Ok(dtos);
    }

    private static ProcesoDto ToDto(ProcesoSeleccion p) => new(
        p.Id, p.Nombre, p.Descripcion, p.Puesto, (int)p.Estado, p.Estado.ToString(),
        p.FechaLimite, p.CreadorId, p.Creador.Nombre,
        p.Candidatos.Count, p.Evaluaciones.Count, p.CreatedAt);
}

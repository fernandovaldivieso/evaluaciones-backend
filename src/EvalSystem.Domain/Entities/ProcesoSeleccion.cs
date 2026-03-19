using EvalSystem.Domain.Common;
using EvalSystem.Domain.Enums;

namespace EvalSystem.Domain.Entities;

public class ProcesoSeleccion : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Puesto { get; set; }
    public EstadoProceso Estado { get; set; } = EstadoProceso.Abierto;
    public DateTime? FechaLimite { get; set; }

    public Guid CreadorId { get; set; }

    // Navigation
    public Usuario Creador { get; set; } = null!;
    public ICollection<ProcesoCandidato> Candidatos { get; set; } = new List<ProcesoCandidato>();
    public ICollection<ProcesoEvaluacion> Evaluaciones { get; set; } = new List<ProcesoEvaluacion>();
}

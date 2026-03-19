using EvalSystem.Domain.Common;

namespace EvalSystem.Domain.Entities;

public class ProcesoCandidato : BaseEntity
{
    public Guid ProcesoId { get; set; }
    public Guid CandidatoId { get; set; }

    // Navigation
    public ProcesoSeleccion Proceso { get; set; } = null!;
    public Usuario Candidato { get; set; } = null!;
}

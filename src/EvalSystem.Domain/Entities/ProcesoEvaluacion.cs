using EvalSystem.Domain.Common;

namespace EvalSystem.Domain.Entities;

public class ProcesoEvaluacion : BaseEntity
{
    public Guid ProcesoId { get; set; }
    public Guid EvaluacionId { get; set; }

    // Navigation
    public ProcesoSeleccion Proceso { get; set; } = null!;
    public Evaluacion Evaluacion { get; set; } = null!;
}

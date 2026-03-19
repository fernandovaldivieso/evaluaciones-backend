using EvalSystem.Domain.Common;

namespace EvalSystem.Domain.Entities;

public class EvaluacionSeccion : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int Orden { get; set; }

    public Guid EvaluacionId { get; set; }

    // Navigation
    public Evaluacion Evaluacion { get; set; } = null!;
    public ICollection<Pregunta> Preguntas { get; set; } = new List<Pregunta>();
}

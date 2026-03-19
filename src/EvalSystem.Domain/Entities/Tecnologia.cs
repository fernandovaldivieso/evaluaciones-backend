using EvalSystem.Domain.Common;

namespace EvalSystem.Domain.Entities;

public class Tecnologia : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Activa { get; set; } = true;

    // Navigation
    public ICollection<Evaluacion> Evaluaciones { get; set; } = new List<Evaluacion>();
}

using EvalSystem.Domain.Common;
using EvalSystem.Domain.Enums;

namespace EvalSystem.Domain.Entities;

public class Evaluacion : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public NivelDificultad Nivel { get; set; }
    public int TiempoLimiteMinutos { get; set; }
    public bool Activa { get; set; } = true;

    public Guid TecnologiaId { get; set; }

    // Navigation
    public Tecnologia Tecnologia { get; set; } = null!;
    public ICollection<EvaluacionSeccion> Secciones { get; set; } = new List<EvaluacionSeccion>();
    public ICollection<SesionEvaluacion> Sesiones { get; set; } = new List<SesionEvaluacion>();
    public ICollection<ProcesoEvaluacion> ProcesoEvaluaciones { get; set; } = new List<ProcesoEvaluacion>();
}

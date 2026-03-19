using EvalSystem.Domain.Common;
using EvalSystem.Domain.Enums;

namespace EvalSystem.Domain.Entities;

public class SesionEvaluacion : BaseEntity
{
    public EstadoSesion Estado { get; set; } = EstadoSesion.Pendiente;
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public int? ScoreObtenido { get; set; }
    public int ScoreMaximo { get; set; }

    public Guid CandidatoId { get; set; }
    public Guid EvaluacionId { get; set; }
    public Guid? ProcesoId { get; set; }

    // Navigation
    public Usuario Candidato { get; set; } = null!;
    public Evaluacion Evaluacion { get; set; } = null!;
    public ProcesoSeleccion? Proceso { get; set; }
    public ICollection<RespuestaCandidato> Respuestas { get; set; } = new List<RespuestaCandidato>();
    public ResultadoEvaluacion? Resultado { get; set; }
}

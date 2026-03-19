using EvalSystem.Domain.Common;
using EvalSystem.Domain.Enums;

namespace EvalSystem.Domain.Entities;

public class Pregunta : BaseEntity
{
    public string Texto { get; set; } = string.Empty;
    public TipoPregunta Tipo { get; set; }
    public int Puntaje { get; set; }
    public int TiempoSegundos { get; set; }
    public int Orden { get; set; }
    public string? Explicacion { get; set; }

    public Guid SeccionId { get; set; }

    // Navigation
    public EvaluacionSeccion Seccion { get; set; } = null!;
    public ICollection<OpcionRespuesta> Opciones { get; set; } = new List<OpcionRespuesta>();
    public ICollection<RespuestaCandidato> Respuestas { get; set; } = new List<RespuestaCandidato>();
}

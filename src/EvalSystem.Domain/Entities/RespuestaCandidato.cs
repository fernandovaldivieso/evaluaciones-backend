using EvalSystem.Domain.Common;

namespace EvalSystem.Domain.Entities;

public class RespuestaCandidato : BaseEntity
{
    public string Respuesta { get; set; } = string.Empty;
    public int TiempoRespuestaSegundos { get; set; }
    public bool? EsCorrecta { get; set; }
    public int? PuntajeObtenido { get; set; }
    public string? ComentarioRevisor { get; set; }

    public Guid SesionId { get; set; }
    public Guid PreguntaId { get; set; }
    public Guid? OpcionSeleccionadaId { get; set; }

    // Navigation
    public SesionEvaluacion Sesion { get; set; } = null!;
    public Pregunta Pregunta { get; set; } = null!;
    public OpcionRespuesta? OpcionSeleccionada { get; set; }
}

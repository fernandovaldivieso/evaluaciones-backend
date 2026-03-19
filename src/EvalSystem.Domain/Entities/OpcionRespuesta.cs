using EvalSystem.Domain.Common;

namespace EvalSystem.Domain.Entities;

public class OpcionRespuesta : BaseEntity
{
    public string Texto { get; set; } = string.Empty;
    public bool EsCorrecta { get; set; }
    public int Orden { get; set; }

    public Guid PreguntaId { get; set; }

    // Navigation
    public Pregunta Pregunta { get; set; } = null!;
}

using EvalSystem.Domain.Common;

namespace EvalSystem.Domain.Entities;

public class ResultadoEvaluacion : BaseEntity
{
    public decimal ScoreTotal { get; set; }
    public string? ScorePorSeccion { get; set; }
    public string? BrechasIdentificadas { get; set; }
    public string? RecomendacionIA { get; set; }
    public string? FortalezasIdentificadas { get; set; }
    public DateTime FechaAnalisis { get; set; }

    public Guid SesionId { get; set; }

    // Navigation
    public SesionEvaluacion Sesion { get; set; } = null!;
}

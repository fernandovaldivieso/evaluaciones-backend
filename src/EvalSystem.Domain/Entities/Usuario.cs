using EvalSystem.Domain.Common;
using EvalSystem.Domain.Enums;

namespace EvalSystem.Domain.Entities;

public class Usuario : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; }
    public bool Activo { get; set; } = true;

    // Navigation
    public ICollection<SesionEvaluacion> Sesiones { get; set; } = new List<SesionEvaluacion>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

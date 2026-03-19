using EvalSystem.Domain.Common;

namespace EvalSystem.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expira { get; set; }
    public bool Revocado { get; set; }
    public Guid UsuarioId { get; set; }

    // Navigation
    public Usuario Usuario { get; set; } = null!;
}

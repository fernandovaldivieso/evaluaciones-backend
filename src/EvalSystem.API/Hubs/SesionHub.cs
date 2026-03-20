using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EvalSystem.API.Hubs;

[Authorize]
public class SesionHub : Hub
{
    /// <summary>
    /// Candidato se une al grupo de su sesión para recibir notificaciones.
    /// </summary>
    public async Task JoinSesion(string sesionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"sesion-{sesionId}");
    }

    /// <summary>
    /// Candidato sale del grupo de su sesión.
    /// </summary>
    public async Task LeaveSesion(string sesionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"sesion-{sesionId}");
    }
}

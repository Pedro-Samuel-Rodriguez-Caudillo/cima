using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Volo.Abp.AspNetCore.SignalR;

namespace cima.Hubs;

/// <summary>
/// SignalR Hub para notificaciones en tiempo real.
/// Permite notificar a usuarios sobre nuevos contactos, listings publicados, etc.
/// </summary>
[Authorize]
public class CimaNotificationHub : AbpHub
{
    /// <summary>
    /// Grupo para administradores
    /// </summary>
    public const string AdminGroup = "Admins";

    /// <summary>
    /// Grupo para arquitectos
    /// </summary>
    public const string ArchitectGroup = "Architects";

    public override async Task OnConnectedAsync()
    {
        // Agregar usuario a grupos según sus roles
        if (CurrentUser.IsInRole("admin"))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, AdminGroup);
        }

        // Todos los usuarios autenticados van al grupo de arquitectos
        if (CurrentUser.IsAuthenticated)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, ArchitectGroup);
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Enviar notificación de nuevo contacto a admins
    /// </summary>
    public async Task NotifyNewContactRequest(string customerName, string propertyTitle)
    {
        await Clients.Group(AdminGroup).SendAsync(
            "ReceiveNewContactNotification",
            customerName,
            propertyTitle);
    }

    /// <summary>
    /// Enviar notificación de listing publicado
    /// </summary>
    public async Task NotifyListingPublished(string listingTitle)
    {
        await Clients.All.SendAsync(
            "ReceiveListingPublished",
            listingTitle);
    }
}

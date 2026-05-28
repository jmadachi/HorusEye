using Microsoft.AspNetCore.SignalR;

namespace HorusEye.Api.Hubs;

public class MovimientosHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public async Task JoinDashboardGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Dashboard");
    }
}

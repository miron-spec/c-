using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace MatchApp.Backend.SignalR;

[Authorize]
public class ChatHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var profileId = Context.User?.FindFirstValue("profile_id");
        if (profileId != null)
            await Groups.AddToGroupAsync(Context.ConnectionId, $"profile:{profileId}");
        await base.OnConnectedAsync();
    }

    public Task JoinMatch(Guid matchId) => Groups.AddToGroupAsync(Context.ConnectionId, $"match:{matchId}");
    public Task LeaveMatch(Guid matchId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, $"match:{matchId}");
}

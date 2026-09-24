using MatchApp.Backend.Data;
using MatchApp.Backend.Entities;
using MatchApp.Backend.SignalR;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace MatchApp.Backend.Services;

public class NotificationService(AppDbContext db, IHubContext<ChatHub> hub)
{
    public async Task CreateAsync(Guid receiverProfileId, string type, string title, string body,
        string? dataJson, CancellationToken ct)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(), ReceiverProfileId = receiverProfileId,
            Type = type, Title = title, Body = body, DataJson = dataJson,
            CreatedAt = DateTime.UtcNow
        };
        db.Notifications.Add(notification);
        await db.SaveChangesAsync(ct);
        await hub.Clients.Group($"profile:{receiverProfileId}").SendAsync("notification", new
        {
            notification.Id, notification.Type, notification.Title, notification.Body,
            notification.DataJson, notification.CreatedAt
        }, ct);
    }
}

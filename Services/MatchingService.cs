using System.Text.Json;
using MatchApp.Backend.Data;
using MatchApp.Backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace MatchApp.Backend.Services;

public class MatchingService(AppDbContext db, NotificationService notifications)
{
    public async Task<(bool isNewLike, bool isMutual, Guid? matchId)> LikeAsync(
        Guid senderId, Guid receiverId, CancellationToken ct)
    {
        if (senderId == receiverId) throw new InvalidOperationException("You cannot like yourself.");

        var receiverExists = await db.UserProfiles.AnyAsync(x => x.Id == receiverId, ct);
        if (!receiverExists) throw new KeyNotFoundException("Profile not found.");

        var existing = await db.Likes.FirstOrDefaultAsync(
            x => x.SenderProfileId == senderId && x.ReceiverProfileId == receiverId, ct);
        if (existing != null)
        {
            var currentMatch = await FindMatchAsync(senderId, receiverId, ct);
            return (false, currentMatch != null, currentMatch?.Id);
        }

        db.Likes.Add(new Like
        {
            Id = Guid.NewGuid(), SenderProfileId = senderId,
            ReceiverProfileId = receiverId, CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);

        var reverse = await db.Likes.AnyAsync(
            x => x.SenderProfileId == receiverId && x.ReceiverProfileId == senderId, ct);

        if (!reverse)
        {
            await notifications.CreateAsync(receiverId, "like", "Нова симпатія",
                "Хтось вподобав вашу анкету.", JsonSerializer.Serialize(new { profileId = senderId }), ct);
            return (true, false, null);
        }

        var match = await FindMatchAsync(senderId, receiverId, ct);
        if (match == null)
        {
            match = new Match
            {
                Id = Guid.NewGuid(), UserProfileId = senderId,
                MatchedProfileId = receiverId, CreatedAt = DateTime.UtcNow, IsActive = true
            };
            db.Matches.Add(match);
            await db.SaveChangesAsync(ct);

            await notifications.CreateAsync(senderId, "match", "Взаємна симпатія",
                "Ви сподобалися одне одному.", JsonSerializer.Serialize(new { matchId = match.Id }), ct);
            await notifications.CreateAsync(receiverId, "match", "Взаємна симпатія",
                "Ви сподобалися одне одному.", JsonSerializer.Serialize(new { matchId = match.Id }), ct);
        }

        return (true, true, match.Id);
    }

    public async Task<Match?> FindMatchAsync(Guid a, Guid b, CancellationToken ct) =>
        await db.Matches.FirstOrDefaultAsync(x =>
            (x.UserProfileId == a && x.MatchedProfileId == b) ||
            (x.UserProfileId == b && x.MatchedProfileId == a), ct);
}

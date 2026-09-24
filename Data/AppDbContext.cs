using MatchApp.Backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace MatchApp.Backend.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
    public DbSet<ProfilePhoto> ProfilePhotos => Set<ProfilePhoto>();
    public DbSet<Interest> Interests => Set<Interest>();
    public DbSet<SearchPreference> SearchPreferences => Set<SearchPreference>();
    public DbSet<Like> Likes => Set<Like>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAccount>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Email).IsUnique();
            e.HasIndex(x => x.ProfileId).IsUnique();
            e.Property(x => x.Email).HasMaxLength(320).IsRequired();
            e.Property(x => x.PasswordHash).IsRequired();
            e.HasOne(x => x.Profile).WithOne().HasForeignKey<UserAccount>(x => x.ProfileId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserProfile>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.UserId).IsUnique();
            e.Property(x => x.Name).HasMaxLength(80).IsRequired();
            e.Property(x => x.City).HasMaxLength(100);
            e.Property(x => x.Bio).HasMaxLength(2000);
        });

        modelBuilder.Entity<ProfilePhoto>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.UserProfileId, x.SortOrder }).IsUnique();
            e.HasOne(x => x.UserProfile).WithMany(x => x.Photos)
                .HasForeignKey(x => x.UserProfileId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Interest>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Name).IsUnique();
            e.Property(x => x.Name).HasMaxLength(60).IsRequired();
        });

        modelBuilder.Entity<UserProfile>()
            .HasMany(x => x.Interests)
            .WithMany(x => x.UserProfiles)
            .UsingEntity(j => j.ToTable("UserInterests"));

        modelBuilder.Entity<SearchPreference>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.UserProfileId).IsUnique();
            e.HasOne(x => x.UserProfile).WithOne(x => x.SearchPreference)
                .HasForeignKey<SearchPreference>(x => x.UserProfileId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Like>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.SenderProfileId, x.ReceiverProfileId }).IsUnique();
            e.HasOne(x => x.SenderProfile).WithMany().HasForeignKey(x => x.SenderProfileId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ReceiverProfile).WithMany().HasForeignKey(x => x.ReceiverProfileId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Match>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.UserProfileId, x.MatchedProfileId }).IsUnique();
            e.HasOne(x => x.UserProfile).WithMany().HasForeignKey(x => x.UserProfileId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.MatchedProfile).WithMany().HasForeignKey(x => x.MatchedProfileId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Message>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.MatchId, x.SentAt });
            e.HasOne(x => x.Match).WithMany(x => x.Messages).HasForeignKey(x => x.MatchId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.SenderProfile).WithMany().HasForeignKey(x => x.SenderProfileId).OnDelete(DeleteBehavior.Restrict);
            e.Property(x => x.Text).HasMaxLength(5000);
        });

        modelBuilder.Entity<Report>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.ReporterProfileId, x.ReportedProfileId });
            e.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
        });

        modelBuilder.Entity<Notification>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.ReceiverProfileId, x.CreatedAt });
            e.Property(x => x.Title).HasMaxLength(150).IsRequired();
            e.Property(x => x.Body).HasMaxLength(500).IsRequired();
        });
    }
}

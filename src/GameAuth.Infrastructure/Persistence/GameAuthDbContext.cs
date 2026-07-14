using GameAuth.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameAuth.Infrastructure.Persistence;

public class GameAuthDbContext : DbContext
{
    public GameAuthDbContext(DbContextOptions<GameAuthDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Credential> Credentials => Set<Credential>();
    public DbSet<MfaSettings> MfaSettings => Set<MfaSettings>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure snake_case table names
        modelBuilder.Entity<User>().ToTable("users");
        modelBuilder.Entity<Credential>().ToTable("credentials");
        modelBuilder.Entity<MfaSettings>().ToTable("mfa_settings");
        modelBuilder.Entity<Session>().ToTable("sessions");
        modelBuilder.Entity<AuditLog>().ToTable("audit_logs");

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Username).HasColumnName("username").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();

            entity.HasOne(e => e.Credential)
                .WithOne(c => c.User)
                .HasForeignKey<Credential>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.MfaSettings)
                .WithOne(m => m.User)
                .HasForeignKey<MfaSettings>(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Sessions)
                .WithOne(s => s.User)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.AuditLogs)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Credential configuration
        modelBuilder.Entity<Credential>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // MfaSettings configuration
        modelBuilder.Entity<MfaSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.MfaType).HasColumnName("mfa_type").HasMaxLength(50);
            entity.Property(e => e.MfaSecret).HasColumnName("mfa_secret").HasMaxLength(255);
            entity.Property(e => e.BackupCodes).HasColumnName("backup_codes");
            entity.Property(e => e.Verified).HasColumnName("verified").HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(e => e.UserId).IsUnique();
        });

        // Session configuration
        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.SessionId).HasColumnName("session_id").HasMaxLength(255).IsRequired();
            entity.Property(e => e.RefreshToken).HasColumnName("refresh_token").HasMaxLength(500).IsRequired();
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.IpAddress).HasColumnName("ip_address");
            entity.Property(e => e.UserAgent).HasColumnName("user_agent");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(e => e.SessionId).IsUnique();
        });

        // AuditLog configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.EventType).HasColumnName("event_type").HasMaxLength(100).IsRequired();
            entity.Property(e => e.EventSource).HasColumnName("event_source").HasMaxLength(50).IsRequired();
            entity.Property(e => e.IpAddress).HasColumnName("ip_address");
            entity.Property(e => e.UserAgent).HasColumnName("user_agent");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50);
            entity.Property(e => e.Timestamp).HasColumnName("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Composite indexes per README
            entity.HasIndex(e => new { e.UserId, e.Timestamp }).HasDatabaseName("idx_audit_logs_user_id_timestamp");
            entity.HasIndex(e => new { e.EventType, e.Timestamp }).HasDatabaseName("idx_audit_logs_event_type_timestamp");
        });
    }
}

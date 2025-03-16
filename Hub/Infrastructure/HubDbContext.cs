using Microsoft.EntityFrameworkCore;
using Hub.Domain;

namespace Hub.Infrastructure
{
    public class HubDbContext : DbContext
    {
        public HubDbContext(DbContextOptions<HubDbContext> options)
            : base(options)
        {
        }

        public DbSet<Tab> Tabs { get; set; }
        public DbSet<ApplicationOS> Applications { get; set; }
        public DbSet<TimeEntry> TimeEntries { get; set; }
        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<Journal> Journals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Tab>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Title)
                      .IsRequired()
                      .HasMaxLength(200);
                entity.Property(t => t.Url)
                      .IsRequired();
                entity.Property(t => t.Date)
                      .IsRequired();
            });

            modelBuilder.Entity<ApplicationOS>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Title)
                      .IsRequired()
                      .HasMaxLength(200);
                entity.Property(a => a.ProcessName)
                      .IsRequired()
                      .HasMaxLength(100);
                entity.Property(a => a.Date)
                      .IsRequired();
            });

            modelBuilder.Entity<TimeEntry>(entity =>
            {
                entity.HasKey(te => te.Id);
                entity.Property(te => te.Date)
                      .IsRequired();
                entity.Property(te => te.Duration)
                      .IsRequired();
                entity.Property(te => te.CurrentSessionStart)
                      .IsRequired(false);

                entity.HasOne(te => te.Tab)
                      .WithMany(t => t.Sessions)
                      .HasForeignKey(te => te.TabId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .IsRequired(false);

                entity.HasOne(te => te.Application)
                      .WithMany(a => a.Sessions)
                      .HasForeignKey(te => te.ApplicationId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .IsRequired(false);
            });

            modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Title)
                      .IsRequired()
                      .HasMaxLength(200);
                entity.Property(t => t.Description)
                      .IsRequired();
                entity.Property(t => t.Priority)
                      .IsRequired();
                entity.Property(t => t.Status)
                      .IsRequired();
                entity.Property(t => t.CreatedAt)
                      .IsRequired();
                entity.Property(t => t.UpdatedAt)
                      .IsRequired(false);
            });

            modelBuilder.Entity<Journal>(entity =>
            {
                entity.HasKey(j => j.Id);
                entity.Property(j => j.Content)
                      .IsRequired()
                      .HasMaxLength(2000);
                entity.Property(j => j.Created)
                      .IsRequired();
                entity.Property(j => j.Updated)
                      .IsRequired(false);
                entity.HasIndex(j => j.Created)
                      .IsUnique();
            });

        }
    }
}

using Microsoft.EntityFrameworkCore;
using MODEL.Entities;

namespace MODEL
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<TaskHeader> TaskHeaders { get; set; }
        public DbSet<TaskDetail> TaskDetails { get; set; }
        public DbSet<FileAttachment> FileAttachments { get; set; }
        public DbSet<SystemLog> SystemLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TaskHeader>()
                .HasMany(t => t.TaskDetails)
                .WithOne(d => d.TaskHeader)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskHeader>()
                .HasMany(t => t.FileAttachments)
                .WithOne(f => f.TaskHeader)
                .HasForeignKey(f => f.TaskId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskHeader>()
                .HasIndex(t => t.TaskCode)
                .IsUnique();
        }
    }
}

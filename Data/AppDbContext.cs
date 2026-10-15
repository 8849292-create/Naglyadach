using System;
using Microsoft.EntityFrameworkCore;
using Naglyadach.Models;

namespace Naglyadach.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<LaunchRecord> LaunchRecord { get; set; }
        public DbSet<ProcessSample> ProcessSample { get; set; }
        public DbSet<LaunchProfile> LaunchProfile { get; set; }
        public DbSet<Snapshot> Snapshots { get; set; }       
        public DbSet<WatchRules> WatchRules { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {  }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<LaunchProfile>(entity => 
            {
                entity.ToTable("LaunchProfile");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
                entity.HasIndex(x => x.Name).IsUnique();
                entity.Property(x => x.FileName).HasMaxLength(500).IsRequired();
                entity.Property(x => x.Arguments).HasMaxLength(1000);
                entity.Property(x => x.MaxInstances).HasDefaultValue(1);
                //entity.HasCheckConstraint("CK_LaunchProfile_MaxInstances", "[MaxInstances] > 0");
            });

            modelBuilder.Entity<LaunchRecord>(entity =>
            {
                entity.ToTable("LaunchRecord");
                entity.HasKey(x => x.Id);
                entity.HasOne(x => x.Profile)
                        .WithMany(x => x.LaunchRecords)
                        .HasForeignKey(x => x.ProfileId)
                        .OnDelete(DeleteBehavior.Cascade);

            });
            modelBuilder.Entity<Snapshot>(entity =>
            {
                entity.ToTable("Snapshot");
                entity.HasKey(x => x.Id);
                //entity.HasCheckConstraint("CK_Snapshot_ProcessCount","[ProcessCount] >= 0");

            });
            modelBuilder.Entity<ProcessSample> (entity =>
            {
                entity.ToTable("ProcessSample");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
                entity.HasOne(x => x.Snapshot)
                    .WithMany(x => x.ProcessSamples)
                    .HasForeignKey(x => x.SnapshotId)
                    .OnDelete(DeleteBehavior.Cascade);
               // entity.HasCheckConstraint("CK_ProcessSample_WorkingSet", "[WorkingSetMb] >= 0");

            });
            modelBuilder.Entity<WatchRules>(entity =>
            {
                entity.ToTable("WatchRules");
                entity.HasKey(x => x.Id);
                entity.Property(x =>x.ProcessName).HasMaxLength(100).IsRequired();
                entity.Property(x => x.Action).HasConversion<string>().HasMaxLength(20).IsRequired();
                //entity.HasCheckConstraint("CK_WatchRule_MaxMemory", "[MaxMemoryMb] >= 0");

            });
        }       
    }
}


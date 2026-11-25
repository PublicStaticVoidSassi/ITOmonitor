using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using sutnance.Areas.Identity.Data;
using sutnance.Models;

namespace sutnance.Data;

public class sutnanceContext : IdentityDbContext<sutnanceUser>
{
    public DbSet<Machine> Machines { get; set; }
    public DbSet<MachineType> MachineTypes {get;set;}
    public DbSet<Feature> Features {get;set;}
    public DbSet<CalleeFunction> CalleeFunctions { get; set; }
    // public DbSet<comprenant> comprenants { get; set; }
    public DbSet<Alert> alerts { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<Historique> Historiques { get; set; }
    public DbSet<Note> Notes { get; set; }

    public sutnanceContext(DbContextOptions<sutnanceContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Note>(entity => {
            entity.ToTable("notes");
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Content).HasColumnName("content");
            entity.Property(n => n.HistoriqueId).HasColumnName("HistoriqueId");
        });
            modelBuilder.Entity<Historique>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("historique", "dbo");

            entity.Property(h => h.Id).HasColumnName("Id");
            entity.Property(h => h.MachineId).HasColumnName("machineid");
            entity.Property(h => h.State).HasColumnName("state");
            entity.Property(h => h.From).HasColumnName("From");
            entity.Property(h => h.Until).HasColumnName("Until");
            entity.Ignore(h => h.Note);
        });
        // =====================
        // Machine → Reports
        // =====================
        modelBuilder.Entity<Report>()
            .HasOne(r => r.Machine)
            .WithMany()
            .HasForeignKey(r => r.MachineId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Machine>()
            .HasOne(m => m.Type)
            .WithMany()
            .HasForeignKey(m => m.TypeId);
        modelBuilder.Entity<MachineType>()
            .HasMany(t => t.Features)
            .WithMany();
        modelBuilder.Entity<Feature>()
            .HasMany<CalleeFunction>(f => f.Functions)
            .WithOne()
            .HasForeignKey(c => c.FeatureId);
        //     .WithMany();
        // .HasForeignKey(f => f.MachineTypeId);
        // modelBuilder.Entity<CalleeFunction>()
        //     .HasOne<Feature>(c => c.Feature)
        //     .WithMany()
        //     .HasForeignKey(c => c.FeatureId);
    }
}

using System;
using Microsoft.EntityFrameworkCore;
using StudyFlow.Models;

namespace StudyFlow.Bus.Database.Context;
public class StudyFlowContext : DbContext
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<Goal> Goals => Set<Goal>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    { 
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\MSSQLLocalDB;Database=StudyFlow;TrustServerCertificate=True;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StudyFlowContext).Assembly);
    }
}

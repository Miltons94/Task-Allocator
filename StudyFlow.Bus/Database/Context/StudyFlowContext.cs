using System;
using Microsoft.EntityFrameworkCore;
using StudyFlow.Models;

namespace StudyFlow.Bus.Database.Context;
public class StudyFlowContext : DbContext
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Use SQLite database provider with a connection string
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StudyFlowContext).Assembly);
    }
}

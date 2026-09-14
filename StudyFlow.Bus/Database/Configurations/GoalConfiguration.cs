using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyFlow.Models;
using System;

namespace StudyFlow.Bus.Database.Configurations;
public class GoalConfiguration : IEntityTypeConfiguration<Goal>
{
    public void Configure(EntityTypeBuilder<Goal> builder)
    {
        builder.HasKey(g => g.ID);
        builder.Property(g => g.Name)
               .IsRequired()
               .HasMaxLength(100);
        builder.Property(g => g.Description)
               .IsRequired(false)
               .HasMaxLength(250);
        builder.Property(g => g.Category);
        builder.Property(g => g.Priority)
               .IsRequired();
        builder.Property(g => g.StartDate)
               .IsRequired();
        builder.Property(g => g.DueDate)
               .IsRequired();
    }
}

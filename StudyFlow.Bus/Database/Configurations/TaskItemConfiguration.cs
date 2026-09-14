using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyFlow.Models;

namespace StudyFlow.Bus.Database.Configurations;
public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.HasKey(t => t.ID);
        builder.Property(t => t.Name)
               .IsRequired()
               .HasMaxLength(100);
        builder.Property(t => t.Description)
               .IsRequired(false)
               .HasMaxLength(250);
        builder.Property(t => t.Category);
        builder.Property(t => t.Priority)
               .IsRequired();
        builder.Property(t => t.StartDate)
               .IsRequired();
        builder.Property(t => t.DueDate)
               .IsRequired();
    }
}

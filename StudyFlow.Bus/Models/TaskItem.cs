using System;
using StudyFlow.Enums;

namespace StudyFlow.Models;
public class TaskItem
{
    public Guid ID { get; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Category Category { get; set; } = Category.Personal;
    public int Priority { get; set; }
    public DateTime StartDate { get; set; } = DateTime.Now;
    public DateTime DueDate { get; set; }
    public TaskItem() { }
    public TaskItem(string name, string? description, Category category, int priority, DateTime startDate, DateTime dueDate)
    {
        Name = name;
        Description = description;
        Category = category;
        Priority = priority;
        StartDate = startDate;
        DueDate = dueDate;
    }
}
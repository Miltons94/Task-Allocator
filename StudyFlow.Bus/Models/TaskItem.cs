using System;
using System.Diagnostics;
using System.Text.Json;
using StudyFlow.Bus.Helpers;
using StudyFlow.Enums;

namespace StudyFlow.Models;
public class TaskItem
{
    private readonly string _storageFolder = Environment.GetFolderPath(
        Environment.SpecialFolder.ApplicationData);
    private readonly string _appName = "StudyFlow";
    private readonly string _filePath = "tasks.json";

    public Guid ID { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Category Category { get; set; } = Category.Personal;
    public int Priority { get; set; }
    public DateTime StartDate { get; set; } = DateTime.Now;
    public DateTime DueDate { get; set; }
    public bool IsCompleted { get; set; } = false;
    public TaskItem() 
    {
    }

    public TaskItem(
        string name,
        string? description,
        Category category,
        int priority,
        DateTime startDate,
        DateTime dueDate,
        bool isCompleted)
    {
        Name = name;
        Description = description;
        Category = category;
        Priority = priority;
        StartDate = startDate;
        DueDate = dueDate;
        IsCompleted = isCompleted;
    }

    public async void Save() { }
    public async void Delete() { }
    public override string ToString()
    {
        return $"Task: {Name}, Description: {Description}, Due: {DueDate}";
    }
}
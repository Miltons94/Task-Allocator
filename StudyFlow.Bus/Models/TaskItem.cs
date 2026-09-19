using System;
using System.Diagnostics;
using System.Text.Json;
using StudyFlow.Enums;

namespace StudyFlow.Models;
public class TaskItem
{
    private readonly string _storageFolder = Environment.GetFolderPath(
        Environment.SpecialFolder.ApplicationData);
    private readonly string _filePath = "tasks.json";
    public Guid ID { get; } = Guid.NewGuid();
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

    // method to save the task 
    public async void Save()
    {
        var file = GetStorageFile(_filePath);
        if(string.IsNullOrEmpty(file))
            throw new InvalidOperationException("Storage file path is invalid.");
        

        var json = await File.ReadAllTextAsync(file);
        List<TaskItem> tasks = !string.IsNullOrEmpty(json)
            ? System.Text.Json.JsonSerializer.Deserialize<List<TaskItem>>(json)
            ?? []
            :  [];
        tasks.Add(this);
        Debug.WriteLine(tasks.Count);
        var updatedJson = JsonSerializer.Serialize(tasks);
        await File.WriteAllTextAsync(file, updatedJson);
    }

    public async void Delete()
    {
        var file = GetStorageFile(_filePath);
        if (string.IsNullOrEmpty(file))
            throw new InvalidOperationException("Storage file path is invalid.");

        var json = await File.ReadAllTextAsync(file);
        List<TaskItem> tasks = !string.IsNullOrEmpty(json)
            ? System.Text.Json.JsonSerializer.Deserialize<List<TaskItem>>(json)
            ?? []
            :  [];
        tasks.RemoveAll(t => t.ID == this.ID);
        var updatedJson = JsonSerializer.Serialize(tasks);
        await File.WriteAllTextAsync(file, updatedJson);
    }
    private string GetStorageFile(string filePath)
    {
        var appFolder = Path.Combine(_storageFolder, "StudyFlow");
        if (!Directory.Exists(appFolder))
        {
            Directory.CreateDirectory(appFolder);
        }
        var file = Path.Combine(appFolder, filePath);
        if (!File.Exists(file))
        {
            File.Create(file).Dispose();
        }
        return file;
    }

    public override string ToString()
    {
        return $"TaskItem: {Name}, Description: {Description}, Category: {Category}, Priority: {Priority}, StartDate: {StartDate}, DueDate: {DueDate}, IsCompleted: {IsCompleted}";
    }
}
using System;
using System.Diagnostics;
using System.Text.Json;
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

    // method to save the task 
    public async void Save()
    {
        var file = GetStorageFile(_filePath);
        if(string.IsNullOrEmpty(file))
            throw new InvalidOperationException("Storage file path is invalid.");
        

        var json = await File.ReadAllTextAsync(file);
        List<TaskItem> tasks = !string.IsNullOrEmpty(json)
            ? DeserializeTasks(json)
            ?? []
            :  [];

        tasks.Add(this);
        var updatedJson = SerializeTasks(tasks);
        await File.WriteAllTextAsync(file, updatedJson);
    }

    public async void Delete()
    {
        var file = GetStorageFile(_filePath);
        if (string.IsNullOrEmpty(file))
            throw new InvalidOperationException("Storage file path is invalid.");

        var json = await File.ReadAllTextAsync(file);
        List<TaskItem> tasks = !string.IsNullOrEmpty(json)
            ? DeserializeTasks(json)
            ?? []
            :  [];

        tasks.RemoveAll(t => t.ID == this.ID);
        var updatedJson = SerializeTasks(tasks);
        await File.WriteAllTextAsync(file, updatedJson);
    }



    /* 
     * method to serialize a list of tasks to JSON
     */
    private static string SerializeTasks(List<TaskItem> tasks)
        => JsonSerializer.Serialize(tasks);

    /* 
     * method to deserialize JSON to a list of tasks
     */
    private static List<TaskItem> DeserializeTasks(string json)
        => JsonSerializer.Deserialize<List<TaskItem>>(json) ?? [];

    /* 
     * method to get the storage file path
     */
    private string GetStorageFile(string filePath)
    {
        var appFolder = Path.Combine(_storageFolder, _appName);
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
        return JsonSerializer.Serialize(
            this, new JsonSerializerOptions { WriteIndented = true });   
    }
}
using StudyFlow.Bus.Helpers;
using StudyFlow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudyFlow.Bus.Services;
public class TaskStorageService
{
    public async void SaveTask(TaskItem task)
    {
        ArgumentNullException.ThrowIfNull(task, nameof(task));
        var filePath = TaskStoragePath.GetStorageFilePath();
        if (string.IsNullOrEmpty(filePath))
            throw new InvalidOperationException("Storage file path is invalid.");
 
        var json = await File.ReadAllTextAsync(filePath);
        List<TaskItem> tasks = !string.IsNullOrEmpty(json)
            ? JsonSerializer.Deserialize<List<TaskItem>>(json)
            ?? []
            : [];

        if (!tasks.Exists(t => t.ID == task.ID))
        {
            tasks.Add(task);
            var updatedJson = JsonSerializer.Serialize(tasks);
            await File.WriteAllTextAsync(filePath, updatedJson);
        }
    }

    public async void UpdateTask(TaskItem task)
    {
        ArgumentNullException.ThrowIfNull(task, nameof(task));
        var filePath = TaskStoragePath.GetStorageFilePath();
        if (string.IsNullOrEmpty(filePath))
            throw new InvalidOperationException("Storage file path is invalid.");

        var json = await File.ReadAllTextAsync(filePath);
        List<TaskItem> tasks = !string.IsNullOrEmpty(json)
            ? JsonSerializer.Deserialize<List<TaskItem>>(json)
            ?? []
            : [];
        var existing = tasks.FirstOrDefault(t => t.ID.Equals(task.ID));
        if(existing != null)
        {
            tasks.Remove(existing);
            existing = task;
            tasks.Insert(0, existing);
            var updatedJson = JsonSerializer.Serialize(
                tasks, new JsonSerializerOptions { WriteIndented = true});
            await File.WriteAllTextAsync(filePath, updatedJson);
        }
    }

    public async void DeleteTask(Guid id)
    {
        ArgumentNullException.ThrowIfNull(id, nameof(id));
        var filePath = TaskStoragePath.GetStorageFilePath();
        if (string.IsNullOrEmpty(filePath))
            throw new InvalidOperationException("Storage file path is invalid.");

        var json = await File.ReadAllTextAsync(filePath);
        List<TaskItem> tasks = !string.IsNullOrEmpty(json)
            ? JsonSerializer.Deserialize<List<TaskItem>>(json)
            ?? []
            : [];
        tasks.RemoveAll(t => t.ID.Equals(id));
        var updatedJson = JsonSerializer.Serialize(
            tasks, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(filePath, updatedJson);
    }
}



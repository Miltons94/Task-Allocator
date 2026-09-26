using StudyFlow.Bus.Enums;
using StudyFlow.Bus.Helpers;
using StudyFlow.Models;
using System.Diagnostics;
using System.Text.Json;

namespace StudyFlow.Bus.Services;
public class TaskStorageService
{
    private static readonly SemaphoreSlim _fileLock = new(1, 1);
    private static readonly JsonSerializerOptions options = new() { WriteIndented = true };

    public async void SaveTask(TaskItem task)
    {
        ArgumentNullException.ThrowIfNull(task, nameof(task));
        var filePath = TaskStoragePath.GetStorageFilePath();
        if (string.IsNullOrEmpty(filePath))
            throw new InvalidOperationException("Storage file path is invalid.");
        try
        {
            await _fileLock.WaitAsync();
            var json = await File.ReadAllTextAsync(filePath);
            List<TaskItem> tasks = await GetTasksAsync(json);

            if (!tasks.Exists(t => t.ID == task.ID))
            {
                task.State = TaskState.Saved;
                tasks.Add(task);
                var updatedJson = JsonSerializer.Serialize(
                    tasks, 
                    options
                );
                await File.WriteAllTextAsync(filePath, updatedJson);
            }
        }
        catch(Exception ex)
        {
            Debug.WriteLine($"An error occured, {ex.Message}");
            throw;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async void UpdateTask(TaskItem task)
    {
        ArgumentNullException.ThrowIfNull(task, nameof(task));
        var filePath = TaskStoragePath.GetStorageFilePath();
        if (string.IsNullOrEmpty(filePath))
            throw new InvalidOperationException("Storage file path is invalid.");

        try
        {
            await _fileLock.WaitAsync();
            var json = await File.ReadAllTextAsync(filePath);
            List<TaskItem> tasks = await GetTasksAsync(json);

            var existing = tasks.FirstOrDefault(t => t.ID.Equals(task.ID));
            if (existing != null)
            {
                tasks.Remove(existing);
                existing = task;
                existing.State = TaskState.Saved;
                tasks.Insert(0, existing);

                var updatedJson = JsonSerializer.Serialize(
                    tasks, 
                    options
                );
                await File.WriteAllTextAsync(filePath, updatedJson);
            }
        }
        catch(Exception ex)
        {
            Debug.WriteLine($"Task updateb failed, {ex.Message}");
            throw;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async void DeleteTask(Guid id)
    {
        ArgumentNullException.ThrowIfNull(id, nameof(id));
        var filePath = TaskStoragePath.GetStorageFilePath();
        if (string.IsNullOrEmpty(filePath))
            throw new InvalidOperationException("Storage file path is invalid.");

        try
        {
            await _fileLock.WaitAsync();
            var json = await File.ReadAllTextAsync(filePath);
            List<TaskItem> tasks = await GetTasksAsync(json);

            var target = tasks.FirstOrDefault(t => t.ID.Equals(id));
            if (target != null)
            {
                target.State = TaskState.Deleted;
                tasks.Remove(target);
                var updatedJson = JsonSerializer.Serialize(
                    tasks, 
                    options
                );
                await File.WriteAllTextAsync(filePath, updatedJson);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to save task, {ex.Message}");
            throw;
        }
    }
    private static async Task<List<TaskItem>> GetTasksAsync(string json)
    {
        try
        {
            await _fileLock.WaitAsync();
            var tasks = !string.IsNullOrEmpty(json)
                ? JsonSerializer.Deserialize<List<TaskItem>>(json)
                ?? []
                :  [];
            return tasks;
        }
        catch(Exception ex)
        {
            Debug.WriteLine($"Failed to load tasks, ${ex.Message}");
            throw;
        }
        finally
        {
            _fileLock.Release();
        }
    }
}



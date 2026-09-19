using StudyFlow.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudyFlow.Bus.Models;

public class AllTasks
{
    private readonly string _filePath = "tasks.json";
    private string _storageFolder = Environment.GetFolderPath(
        Environment.SpecialFolder.ApplicationData);
    public ObservableCollection<TaskItem> Tasks { get; } 
    public AllTasks()
    {
        Tasks = new ObservableCollection<TaskItem>();
    }

    public async Task<ObservableCollection<TaskItem>> GetTasksAsync()
    {
        var file = GetStorageFile(_filePath);
        if(string.IsNullOrEmpty(file))                                                          
            throw new InvalidOperationException("Storage file path is invalid.");

        var json = await File.ReadAllTextAsync(file);
        var tasks = !string.IsNullOrEmpty(json)
            ? JsonSerializer.Deserialize<ObservableCollection<TaskItem>>(json)
            ?? []
            :  [];

        Debug.WriteLine("Loaded tasks from file: " + json);
        foreach (var task in tasks)
            Tasks.Add(task);
        
        return Tasks;

    }
    private string GetStorageFile(string filePath)
    {
        var appFolder = Path.Combine(_storageFolder, "StudyFlow");
        if(!Directory.Exists(appFolder))
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
}

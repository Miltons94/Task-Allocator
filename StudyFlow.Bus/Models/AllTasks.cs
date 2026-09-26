using StudyFlow.Bus.Helpers;
using StudyFlow.Models;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace StudyFlow.Bus.Models;

public class AllTasks
{
    public ObservableCollection<TaskItem> Tasks { get; } 
    public AllTasks()
    {
        Tasks = new ObservableCollection<TaskItem>();
    }
    public async Task<ObservableCollection<TaskItem>> GetTasksAsync()
    {
        var file = TaskStoragePath.GetStorageFilePath();
        if(string.IsNullOrEmpty(file))                                                          
            throw new InvalidOperationException("Storage file path is invalid."); 

        var json = await File.ReadAllTextAsync(file);
        var tasks = !string.IsNullOrEmpty(json)
            ? JsonSerializer.Deserialize<ObservableCollection<TaskItem>>(json)
            ?? []
            :  [];

        foreach (var task in tasks.OrderByDescending(t=>t.StartDate)
                                  .ThenBy(t=>t.IsCompleted))
        {
            Tasks.Add(task);
        }
        return Tasks;
    }
}

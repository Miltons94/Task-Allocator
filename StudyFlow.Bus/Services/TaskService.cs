
using StudyFlow.Interfaces;
using StudyFlow.Models;
using StudyFlow.Repositories;

namespace StudyFlow.Services;

public class TaskService : ITaskService
{
    private readonly TaskRepository taskRepository = new();
    public void CreateTask(TaskItem task)
    {
        if(task is null)
            throw new ArgumentNullException(nameof(task), "Cannot pass a null task!");

        //todo
        /*
        - validation of task data
        - 
        */
        taskRepository.Save(task);
    }

    public void DeleteTask(Guid id)
    {
        var task = taskRepository.GetTask(id)
            ?? throw new InvalidOperationException("Task does not exist!");
        
        taskRepository.Delete(task.ID);
    }

    public TaskItem? GetTask(Guid id)
    {
        if(!taskRepository.Exists(id))
            throw new InvalidOperationException("The task you requested does not exist!");
        
        return taskRepository.GetTask(id);
    }

    public void UpdateTask(TaskItem task)
    {
        if(task is null)
            throw new ArgumentNullException(nameof(task), "The task cannot be null!");

        if(!taskRepository.Exists(task.ID))
            throw new InvalidOperationException("The task you provided does not exist!");

        taskRepository.Update(task);
    }
}
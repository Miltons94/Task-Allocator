using System;
using StudyFlow.Models;

namespace StudyFlow.Interfaces;
public interface ITaskService
{
    void CreateTask(TaskItem task);
    void UpdateTask(TaskItem task);
    void DeleteTask(Guid id);
    TaskItem? GetTask(Guid id); 
}
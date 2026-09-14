
using StudyFlow.Interfaces;
using StudyFlow.Models;

namespace StudyFlow.Repositories;

public class TaskRepository : ITaskRepository
{
    public TaskItem? GetTask(Guid id)
    {
        throw new NotImplementedException();
    }

    public void Save(TaskItem task)
    {
        throw new NotImplementedException();
    }

    public void Update(TaskItem task)
    {
        throw new NotImplementedException();
    }
        public void Delete(Guid id)
    {
        throw new NotImplementedException();
    }

    public bool Exists(Guid id)
    {
        var task = GetTask(id);
        return task != null;
    }
}
using StudyFlow.Models;
namespace StudyFlow.Interfaces;
public interface ITaskRepository
{
    void Save(TaskItem task);
    void Update(TaskItem task);
    void Delete(Guid id);
    TaskItem? GetTask(Guid id);
    bool Exists(Guid id);
}
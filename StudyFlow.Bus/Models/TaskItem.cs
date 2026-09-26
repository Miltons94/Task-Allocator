using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using StudyFlow.Bus.Enums;
using StudyFlow.Enums;

namespace StudyFlow.Models;
public class TaskItem : INotifyPropertyChanged
{
    public Guid ID { get; set; } = Guid.NewGuid();
    private string _name  = string.Empty;
    private string? _description = string.Empty;
    private bool _isCompleted = false;
    public string Name
    {
        get => _name;
        set
        {
            if(_name != value)
            {
                _name = value;
                State = TaskState.Unsaved;
                OnPropertyChanged(nameof(Name));
            }
        }
    }
    public string? Description
    {
        get => _description;
        set
        {
            if(_description != value)
            {
                _description = value;
                State = TaskState.Unsaved;
                OnPropertyChanged(nameof(Description));
            }
        }
    }
    public bool IsCompleted
    {
        get => _isCompleted;
        set
        {
            if(_isCompleted != value)
            {
                _isCompleted = value;
                State = TaskState.Unsaved;
                OnPropertyChanged(nameof(IsCompleted));
            }
        }
    }
    public Category Category { get; set; } = Category.Personal;
    public TaskState State { get; set; } = TaskState.Unset;
    public int Priority { get; set; }
    public DateTimeOffset StartDate { get; set; } = DateTime.Now;
    public DateTimeOffset? DueDate { get; set; }
    public TaskItem() { }
    public TaskItem(
        string name,
        string? description,
        Category category,
        TaskState state,
        int priority,
        DateTimeOffset startDate,
        DateTimeOffset? dueDate,
        bool isCompleted)
    {
        Name = name;
        Description = description;
        Category = category;
        State = state;
        Priority = priority;
        StartDate = startDate;
        DueDate = dueDate;
        IsCompleted = isCompleted;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public override string ToString()
    {
        return $"Task: {Name}, Description: {Description}, Due: {DueDate}";
    }
}
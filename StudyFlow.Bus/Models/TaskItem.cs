using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using StudyFlow.Bus.Enums;
using StudyFlow.Enums;
using System.Collections.ObjectModel;

namespace StudyFlow.Models;
public class TaskItem : INotifyPropertyChanged
{
    public Guid ID { get; set; } = Guid.NewGuid();
    private string _name  = string.Empty;
    private string? _description = string.Empty;
    private bool _isCompleted = false;
    private bool _isExpanded;
    private DateTimeOffset? _dueDate;
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
    public bool HasExplicitDueTime { get; set; }
    public int EstimatedMinutes { get; set; } = 30;
    public int FocusMinutes { get; set; }
    public bool RemindersEnabled { get; set; } = true;
    public int ReminderLeadMinutes { get; set; } = 30;
    public Guid? LinkedEventId { get; set; }
    public string? LinkedEventTitle { get; set; }
    public ObservableCollection<TaskChecklistItem> Checklist { get; set; } = new();
    [JsonIgnore]
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded != value)
            {
                _isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
            }
        }
    }
    public DateTimeOffset StartDate { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset? DueDate
    {
        get => _dueDate;
        set
        {
            if (_dueDate != value)
            {
                _dueDate = value;
                State = TaskState.Unsaved;
                OnPropertyChanged(nameof(DueDate));
                OnPropertyChanged(nameof(DueDateLabel));
                OnPropertyChanged(nameof(IsDueSoon));
            }
        }
    }
    [JsonIgnore]
    public string DueDateLabel
    {
        get
        {
            if (DueDate is not DateTimeOffset dueDate)
            {
                return string.Empty;
            }

            if (!HasExplicitDueTime)
            {
                dueDate = new DateTimeOffset(dueDate.Year, dueDate.Month, dueDate.Day, 9, 0, 0, dueDate.Offset);
            }

            return dueDate.ToString("ddd dd MMM, h:mm tt", CultureInfo.InvariantCulture);
        }
    }
    [JsonIgnore]
    public bool IsDueSoon => DueDate is DateTimeOffset dueDate
        && dueDate.Date < DateTimeOffset.Now.Date.AddDays(3);

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

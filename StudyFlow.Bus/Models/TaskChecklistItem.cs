using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StudyFlow.Models;

public sealed class TaskChecklistItem : INotifyPropertyChanged
{
    private bool _isCompleted;

    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;

    public bool IsCompleted
    {
        get => _isCompleted;
        set
        {
            if (_isCompleted == value)
            {
                return;
            }

            _isCompleted = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

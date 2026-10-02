using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using StudyFlow.Bus.Models;
using StudyFlow.Bus.Interfaces;
using StudyFlow.Bus.Services;
using StudyFlow.Enums;
using StudyFlow.Models;
using StudyFlow.Services;
using StudyFlow.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using StudyFlow.Views.Dialogs;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;

namespace StudyFlow.Views.Pages;
public sealed partial class MainPage : Page
{
    private DialogService _dialogService { get; } = new();
    private AllTasks AllTasks { get; } = new AllTasks();
    private ObservableCollection<TaskItem> Tasks { get; } = new();
    private ObservableCollection<TaskViewModel> ActiveTasks { get; } = new();
    private ObservableCollection<TaskViewModel> CompletedTasks { get; } = new();
    private ObservableCollection<AcademicEvent> Events { get; } = new();
    private ObservableCollection<AcademicEvent> UpcomingEvents { get; } = new();
    private ObservableCollection<AcademicEvent> PastEvents { get; } = new();
    private TaskStorageService _taskStorage { get; } = new();
    private IAcademicEventService _academicEventService { get; } = new AcademicEventService();
    private readonly DispatcherTimer _focusTimer = new() { Interval = TimeSpan.FromSeconds(1) };
    private TaskItem? _focusTask;
    private DateTimeOffset? _focusSegmentStarted;
    private TimeSpan _focusElapsed = TimeSpan.Zero;
    private string _taskScope = "All";
    private string _taskSearch = string.Empty;
    private string _categoryFilter = "Any";
    private string _priorityFilter = "Any";
    private string _sortMode = "Suggested";
    private bool _isUiReady;
    private static readonly TimeSpan FocusDuration = TimeSpan.FromMinutes(25);
    public DateTimeOffset CalendarMinDate { get; } = DateTimeOffset.Now.Date.AddYears(-1);
    public DateTimeOffset CalendarMaxDate { get; } = DateTimeOffset.Now.Date.AddYears(5);
    public MainPage()
    {
        InitializeComponent();
        _isUiReady = true;
        _focusTimer.Tick += FocusTimer_Tick;
        RefreshTaskGroups();
    }

    private async void AddTaskBtn_Click(object sender, RoutedEventArgs e)
    {
        var result = await _dialogService.ShowDialogAsync(new TaskDialog(), XamlRoot);
        if(result == ContentDialogResult.Primary)
        {
            await LoadTasksAsync();
            RefreshTaskAlarms();
        }
    }

    private async void MarkCompleted_Invoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
    {
        if(args.SwipeControl.DataContext is TaskItem task)
        {
            task.IsCompleted = !task.IsCompleted;
            await _taskStorage.UpdateTaskAsync(task);
            RefreshTaskGroups();
            RefreshTaskAlarms();
        }
    }
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is string destination)
        {
            NavigateToSection(destination);
        }
        try
        {
            await LoadTasksAsync();
            RefreshTaskAlarms();
            await LoadAcademicEventsAsync();
        }
        catch (Exception ex)
        {
            await ShowAcademicEventErrorAsync("The academic calendar could not be loaded.", ex);
        }
    }

    private async void DeleteSwipeItem_Invoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
    {
        if(args.SwipeControl.DataContext is TaskItem task)
        {
            Tasks.Remove(task);
            await _taskStorage.DeleteTaskAsync(task.ID);
            RefreshTaskGroups();
            RefreshTaskAlarms();
        }
    }

    private async void EditSwipeItem_Invoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
    {
        if (args.SwipeControl.DataContext is TaskItem task)
        {
            var result = await _dialogService.ShowDialogAsync(new TaskDialog(task), XamlRoot);
            if (result == ContentDialogResult.Primary)
            {
                await LoadTasksAsync();
                RefreshTaskAlarms();
            }
        }
    }

    private async void ToggleButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton btn
            && btn.DataContext is TaskItem task)
        {
            task.IsCompleted = btn.IsChecked == true;
            await _taskStorage.UpdateTaskAsync(task);
            RefreshTaskGroups();
            RefreshTaskAlarms();
        }
    }
    public void NavigateToSection(string destination)
    {
        _taskScope = destination switch
        {
            "Focus" => "Focus",
            "Today" => "Today",
            "Overdue" => "Overdue",
            _ => "All"
        };

        var showCalendar = destination is "Home" or "Calendar";
        var showTasks = destination != "Calendar";
        TasksSection.Visibility = showTasks ? Visibility.Visible : Visibility.Collapsed;
        CalendarSection.Visibility = showCalendar ? Visibility.Visible : Visibility.Collapsed;

        TasksColumn.Width = showTasks ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
        CalendarColumn.Width = showCalendar ? new GridLength(1.2, GridUnitType.Star) : new GridLength(0);
        Grid.SetColumn(CalendarSection, destination == "Calendar" ? 0 : 1);
        Grid.SetColumnSpan(CalendarSection, destination == "Calendar" ? 2 : 1);
        TaskScopeTitle.Text = destination switch
        {
            "Focus" => "Focus queue",
            "Today" => "Due today",
            "Overdue" => "Overdue tasks",
            _ => "My Tasks"
        };
        RefreshTaskGroups();
    }

    private void TaskSearch_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        _taskSearch = sender.Text.Trim();
        RefreshTaskGroups();
    }

    private void TaskOptionsMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem { Tag: string option })
        {
            return;
        }

        var parts = option.Split(':', 2);
        if (parts.Length != 2)
        {
            return;
        }

        switch (parts[0])
        {
            case "Sort":
                _sortMode = parts[1];
                break;
            case "Category":
                _categoryFilter = parts[1];
                break;
            case "Priority":
                _priorityFilter = parts[1];
                break;
        }

        RefreshTaskGroups();
    }

    private async void AddSubtask_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: TaskItem task, Parent: Grid row })
        {
            return;
        }

        var input = row.Children.OfType<TextBox>().FirstOrDefault();
        if (input is not null)
        {
            await AddSubtaskAsync(task, input);
        }
    }

    private async void SubtaskInput_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key != Windows.System.VirtualKey.Enter
            || sender is not TextBox input
            || input.DataContext is not TaskItem task)
        {
            return;
        }

        e.Handled = true;
        await AddSubtaskAsync(task, input);
    }

    private async Task AddSubtaskAsync(TaskItem task, TextBox input)
    {
        var title = input.Text.Trim();
        if (string.IsNullOrWhiteSpace(title)
            || task.Checklist.Any(item => string.Equals(item.Title, title, StringComparison.CurrentCultureIgnoreCase)))
        {
            return;
        }

        task.Checklist.Add(new TaskChecklistItem { Title = title });
        task.IsExpanded = true;
        await _taskStorage.UpdateTaskAsync(task);
        input.Text = string.Empty;
        RefreshTaskGroups();
    }

    private async void StartFocus_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: TaskItem task })
        {
            await StartFocusSessionAsync(task);
        }
    }

    private async void StartSuggestedFocus_Click(object sender, RoutedEventArgs e)
    {
        if (SuggestedNextFocusButton.DataContext is TaskItem task)
        {
            await StartFocusSessionAsync(task);
        }
    }

    private async Task StartFocusSessionAsync(TaskItem task)
    {
        if (task.IsCompleted)
        {
            return;
        }

        if (_focusTask is not null)
        {
            await SaveFocusSessionAsync();
        }

        _focusTask = task;
        _focusElapsed = TimeSpan.Zero;
        _focusSegmentStarted = DateTimeOffset.Now;
        FocusTaskTitle.Text = $"Focus session · {task.Name}";
        FocusSessionBar.Visibility = Visibility.Visible;
        PauseFocusButton.Visibility = Visibility.Visible;
        ResumeFocusButton.Visibility = Visibility.Collapsed;
        _focusTimer.Start();
        UpdateFocusTimerText();
    }

    private void PauseFocus_Click(object sender, RoutedEventArgs e)
    {
        PauseFocusSession();
    }

    private void PauseFocusSession()
    {
        if (_focusSegmentStarted is DateTimeOffset started)
        {
            _focusElapsed += DateTimeOffset.Now - started;
            _focusSegmentStarted = null;
        }

        _focusTimer.Stop();
        PauseFocusButton.Visibility = Visibility.Collapsed;
        ResumeFocusButton.Visibility = Visibility.Visible;
        UpdateFocusTimerText();
    }

    private void ResumeFocus_Click(object sender, RoutedEventArgs e)
    {
        if (_focusTask is null || _focusSegmentStarted.HasValue)
        {
            return;
        }

        _focusSegmentStarted = DateTimeOffset.Now;
        PauseFocusButton.Visibility = Visibility.Visible;
        ResumeFocusButton.Visibility = Visibility.Collapsed;
        _focusTimer.Start();
        UpdateFocusTimerText();
    }

    private async void FinishFocus_Click(object sender, RoutedEventArgs e)
    {
        await SaveFocusSessionAsync();
    }

    private async void FocusTimer_Tick(object? sender, object e)
    {
        UpdateFocusTimerText();
        if (GetCurrentFocusElapsed() >= FocusDuration)
        {
            await SaveFocusSessionAsync();
        }
    }

    private TimeSpan GetCurrentFocusElapsed()
        => _focusElapsed + (_focusSegmentStarted is DateTimeOffset start ? DateTimeOffset.Now - start : TimeSpan.Zero);

    private void UpdateFocusTimerText()
    {
        var remaining = FocusDuration - GetCurrentFocusElapsed();
        if (remaining < TimeSpan.Zero)
        {
            remaining = TimeSpan.Zero;
        }

        FocusTimerText.Text = $"{(int)remaining.TotalMinutes:00}:{remaining.Seconds:00} remaining";
    }

    private async Task SaveFocusSessionAsync()
    {
        if (_focusTask is null)
        {
            return;
        }

        _focusTimer.Stop();
        var task = _focusTask;
        var minutes = (int)Math.Round(GetCurrentFocusElapsed().TotalMinutes, MidpointRounding.AwayFromZero);
        if (minutes > 0)
        {
            task.FocusMinutes += minutes;
            await _taskStorage.UpdateTaskAsync(task);
        }

        _focusTask = null;
        _focusSegmentStarted = null;
        _focusElapsed = TimeSpan.Zero;
        FocusSessionBar.Visibility = Visibility.Collapsed;
        RefreshTaskGroups();
    }

    private async void ChecklistItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox { DataContext: TaskChecklistItem item })
        {
            item.IsCompleted = ((CheckBox)sender).IsChecked == true;
            var owner = Tasks.FirstOrDefault(task => task.Checklist.Any(checklistItem => checklistItem.Id == item.Id));
            if (owner is not null)
            {
                await _taskStorage.UpdateTaskAsync(owner);
                RefreshTaskGroups();
            }
        }
    }

    private async void AddAcademicEvent_Click(object sender, RoutedEventArgs e)
    {
        await ShowAcademicEventDialogAsync();
    }

    private async void EditAcademicEvent_Click(object sender, RoutedEventArgs e)
    {
        if (GetAcademicEvent(sender) is AcademicEvent academicEvent)
        {
            await ShowAcademicEventDialogAsync(academicEvent);
        }
    }

    private async void DeleteAcademicEvent_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: AcademicEvent academicEvent })
        {
            return;
        }

        var confirmation = new ContentDialog
        {
            Title = "Delete event?",
            Content = $"Remove \"{academicEvent.Title}\" from your academic calendar?",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = XamlRoot
        };

        if (await confirmation.ShowAsync() != ContentDialogResult.Primary)
        {
            return;
        }

        try
        {
            await _academicEventService.DeleteEventAsync(academicEvent.Id);
            await LoadAcademicEventsAsync();
        }
        catch (Exception ex)
        {
            await ShowAcademicEventErrorAsync("The event could not be deleted.", ex);
        }
    }

    private static AcademicEvent? GetAcademicEvent(object sender)
        => sender switch
        {
            FrameworkElement { DataContext: AcademicEvent academicEvent } => academicEvent,
            MenuFlyoutItem { Tag: AcademicEvent academicEvent } => academicEvent,
            _ => null
        };

    private async Task ShowAcademicEventDialogAsync(AcademicEvent? existingEvent = null)
    {
        var dialog = new AcademicEventDialog(existingEvent);
        if (await _dialogService.ShowDialogAsync(dialog, XamlRoot) != ContentDialogResult.Primary)
        {
            return;
        }

        try
        {
            await _academicEventService.SaveEventAsync(dialog.Event);
            await LoadAcademicEventsAsync();
        }
        catch (Exception ex)
        {
            await ShowAcademicEventErrorAsync("The event could not be saved.", ex);
        }
    }

    private async Task LoadAcademicEventsAsync()
    {
        var events = await _academicEventService.GetEventsAsync();
        var today = DateTimeOffset.Now.Date;

        Events.Clear();
        UpcomingEvents.Clear();
        PastEvents.Clear();
        foreach (var academicEvent in events)
        {
            Events.Add(academicEvent);
            if ((academicEvent.EndDate ?? academicEvent.StartDate).Date >= today)
            {
                UpcomingEvents.Add(academicEvent);
            }
            else
            {
                PastEvents.Add(academicEvent);
            }
        }

        AcademicCalendar.UpdateLayout();
    }

    private void AcademicCalendar_DayItemChanging(
        CalendarView sender,
        CalendarViewDayItemChangingEventArgs args)
    {
        if (args.InRecycleQueue || args.Item is null)
        {
            return;
        }

        var day = args.Item.Date.Date;
        var densityColors = Events
            .Where(academicEvent =>
            {
                var endDate = (academicEvent.EndDate ?? academicEvent.StartDate).Date;
                return day >= academicEvent.StartDate.Date && day <= endDate;
            })
            .Select(academicEvent => academicEvent.Type switch
            {
                StudyFlow.Bus.Enums.AcademicEventType.Exam => Color.FromArgb(255, 196, 62, 54),
                StudyFlow.Bus.Enums.AcademicEventType.Holiday => Color.FromArgb(255, 36, 132, 93),
                StudyFlow.Bus.Enums.AcademicEventType.Game => Color.FromArgb(255, 123, 86, 180),
                StudyFlow.Bus.Enums.AcademicEventType.Assignment => Color.FromArgb(255, 0, 120, 212),
                StudyFlow.Bus.Enums.AcademicEventType.Activity => Color.FromArgb(255, 196, 124, 36),
                _ => Color.FromArgb(255, 100, 100, 100)
            })
            .Distinct()
            .Take(3)
            .ToList();

        if (densityColors.Count > 0)
        {
            args.Item.SetDensityColors(densityColors);
        }
    }

    private async Task ShowAcademicEventErrorAsync(string message, Exception exception)
    {
        Debug.WriteLine($"{message} {exception}");
        var dialog = new ContentDialog
        {
            Title = "Calendar error",
            Content = $"{message}\n\n{exception.GetType().Name}: {exception.Message}",
            CloseButtonText = "OK",
            XamlRoot = XamlRoot
        };
        await dialog.ShowAsync();
    }

    private async Task LoadTasksAsync()
    {
        var expansionStates = Tasks.ToDictionary(task => task.ID, task => task.IsExpanded);
        Tasks.Clear();
        var tasks = await AllTasks.GetTasksAsync();
        foreach (var task in tasks)
        {
            task.Checklist ??= new ObservableCollection<TaskChecklistItem>();
            task.IsExpanded = expansionStates.TryGetValue(task.ID, out var isExpanded)
                ? isExpanded
                : task.Checklist.Count > 0;
            Tasks.Add(task);
        }
        RefreshTaskGroups();
    }

    private async void CreateEventPrepTasks_Click(object sender, RoutedEventArgs e)
    {
        if (GetAcademicEvent(sender) is not AcademicEvent academicEvent)
        {
            return;
        }

        if (academicEvent.StartDate.Date < DateTimeOffset.Now.Date)
        {
            var pastEventDialog = new ContentDialog
            {
                Title = "Event has passed",
                Content = "Prep tasks can only be created for upcoming events.",
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            };
            await pastEventDialog.ShowAsync();
            return;
        }

        var today = DateTimeOffset.Now.Date;
        var plan = academicEvent.Type switch
        {
            StudyFlow.Bus.Enums.AcademicEventType.Exam => new[]
            {
                (Title: $"Review notes for {academicEvent.Title}", DaysBefore: 3, Minutes: 45),
                (Title: $"Practice for {academicEvent.Title}", DaysBefore: 1, Minutes: 30),
                (Title: $"Gather materials for {academicEvent.Title}", DaysBefore: 0, Minutes: 15)
            },
            StudyFlow.Bus.Enums.AcademicEventType.Assignment => new[]
            {
                (Title: $"Outline {academicEvent.Title}", DaysBefore: 3, Minutes: 25),
                (Title: $"Draft {academicEvent.Title}", DaysBefore: 2, Minutes: 45),
                (Title: $"Review {academicEvent.Title}", DaysBefore: 1, Minutes: 25)
            },
            _ => new[]
            {
                (Title: $"Prepare for {academicEvent.Title}", DaysBefore: 1, Minutes: 25),
                (Title: $"Gather materials for {academicEvent.Title}", DaysBefore: 0, Minutes: 15)
            }
        };

        var existingTitles = Tasks
            .Where(task => task.LinkedEventId == academicEvent.Id)
            .Select(task => task.Name)
            .ToHashSet(StringComparer.CurrentCultureIgnoreCase);
        var created = 0;
        foreach (var step in plan)
        {
            if (existingTitles.Contains(step.Title))
            {
                continue;
            }

            var dueDay = academicEvent.StartDate.Date.AddDays(-step.DaysBefore);
            if (dueDay < today)
            {
                dueDay = today;
            }

            var task = new TaskItem
            {
                Name = step.Title,
                Description = $"Preparation for {academicEvent.Title}.",
                Category = Category.UniversityWork,
                Priority = academicEvent.Type is StudyFlow.Bus.Enums.AcademicEventType.Exam
                    or StudyFlow.Bus.Enums.AcademicEventType.Assignment ? 3 : 2,
                StartDate = DateTimeOffset.Now,
                DueDate = new DateTimeOffset(dueDay.Year, dueDay.Month, dueDay.Day, 9, 0, 0, academicEvent.StartDate.Offset),
                HasExplicitDueTime = true,
                EstimatedMinutes = step.Minutes,
                RemindersEnabled = true,
                ReminderLeadMinutes = 30,
                LinkedEventId = academicEvent.Id,
                LinkedEventTitle = academicEvent.Title
            };
            await _taskStorage.SaveTaskAsync(task);
            existingTitles.Add(step.Title);
            created++;
        }

        if (created > 0)
        {
            await LoadTasksAsync();
            RefreshTaskAlarms();
        }

        var resultDialog = new ContentDialog
        {
            Title = created > 0 ? "Prep plan created" : "Prep plan already exists",
            Content = created > 0
                ? $"Added {created} preparation task{(created == 1 ? "" : "s")} for {academicEvent.Title}."
                : $"All suggested preparation tasks for {academicEvent.Title} are already in your task list.",
            CloseButtonText = "OK",
            XamlRoot = XamlRoot
        };
        await resultDialog.ShowAsync();
    }

    private void RefreshTaskAlarms()
    {
        try
        {
            TaskAlarmService.Reconcile(Tasks);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Task reminders could not be scheduled: {ex}");
        }
    }

    private void RefreshTaskGroups()
    {
        // SelectionChanged can fire while InitializeComponent is still creating
        // named controls. Wait until the visual tree is ready before touching it.
        if (!_isUiReady)
        {
            return;
        }

        ActiveTasks.Clear();
        CompletedTasks.Clear();

        var today = DateTimeOffset.Now.Date;
        IEnumerable<TaskItem> matching = Tasks;
        matching = _taskScope switch
        {
            "Today" => matching.Where(task => task.DueDate?.Date == today),
            "Overdue" => matching.Where(task => task.DueDate is DateTimeOffset dueDate && dueDate.Date < today),
            _ => matching
        };

        if (!string.Equals(_categoryFilter, "Any", StringComparison.Ordinal))
        {
            matching = matching.Where(task => task.Category.ToString() == _categoryFilter);
        }

        if (int.TryParse(_priorityFilter, out var priority))
        {
            matching = matching.Where(task => task.Priority == priority);
        }

        if (!string.IsNullOrWhiteSpace(_taskSearch))
        {
            var query = _taskSearch.Trim();
            matching = matching.Where(task =>
                task.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase)
                || (task.Description?.Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false)
                || task.Category.ToString().Contains(query, StringComparison.CurrentCultureIgnoreCase)
                || (task.LinkedEventTitle?.Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false)
                || task.Checklist.Any(item => item.Title.Contains(query, StringComparison.CurrentCultureIgnoreCase)));
        }

        var matchedTasks = matching.ToList();
        var now = DateTimeOffset.Now;
        var allocated = TaskViewModel.Allocate(matchedTasks, now);
        if (_taskScope == "Focus")
        {
            allocated = allocated.Take(5).ToList();
        }

        var visibleTasks = _sortMode switch
        {
            "DueDate" => allocated.OrderBy(viewModel => viewModel.Task.DueDate ?? DateTimeOffset.MaxValue).ToList(),
            "Priority" => allocated.OrderByDescending(viewModel => viewModel.Task.Priority)
                .ThenBy(viewModel => viewModel.Task.DueDate ?? DateTimeOffset.MaxValue).ToList(),
            "Newest" => allocated.OrderByDescending(viewModel => viewModel.Task.StartDate).ToList(),
            _ => allocated
        };

        foreach (var taskViewModel in visibleTasks)
        {
            ActiveTasks.Add(taskViewModel);
        }

        foreach (var task in matchedTasks
                     .Where(task => task.IsCompleted)
                     .OrderByDescending(task => task.StartDate))
        {
            CompletedTasks.Add(new TaskViewModel(task, now));
        }

        if (allocated.Count > 0)
        {
            var next = allocated[0];
            SuggestedNextTitle.Text = next.Task.Name;
            SuggestedNextReason.Text = $"{next.AllocationReason} · estimated {next.Task.EstimatedMinutes} min";
            SuggestedNextFocusButton.DataContext = next.Task;
            SuggestedNextCard.Visibility = Visibility.Visible;
        }
        else
        {
            SuggestedNextCard.Visibility = Visibility.Collapsed;
        }
    }
}

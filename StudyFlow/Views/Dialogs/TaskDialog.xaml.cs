using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using StudyFlow.Bus.Services;
using StudyFlow.Enums;
using StudyFlow.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace StudyFlow.Views.Dialogs;

public sealed partial class TaskDialog : ContentDialog
{
    private readonly TaskStorageService _taskStorage = new();
    private readonly bool _isEditing;

    public TaskItem TaskItem { get; }

    public TaskDialog(TaskItem? existingTask = null)
    {
        InitializeComponent();

        _isEditing = existingTask is not null;
        TaskItem = existingTask is null
            ? new TaskItem()
            : new TaskItem(
                existingTask.Name,
                existingTask.Description,
                existingTask.Category,
                existingTask.State,
                existingTask.Priority,
                existingTask.StartDate,
                existingTask.DueDate,
                existingTask.IsCompleted)
            {
                ID = existingTask.ID,
                HasExplicitDueTime = existingTask.HasExplicitDueTime,
                EstimatedMinutes = existingTask.EstimatedMinutes,
                FocusMinutes = existingTask.FocusMinutes,
                RemindersEnabled = existingTask.RemindersEnabled,
                ReminderLeadMinutes = existingTask.ReminderLeadMinutes,
                LinkedEventId = existingTask.LinkedEventId,
                LinkedEventTitle = existingTask.LinkedEventTitle,
                Checklist = new ObservableCollection<TaskChecklistItem>(existingTask.Checklist.Select(item => new TaskChecklistItem
                {
                    Id = item.Id,
                    Title = item.Title,
                    IsCompleted = item.IsCompleted
                }))
            };

        Title = _isEditing ? "Edit task" : "Create task";
        PrimaryButtonText = _isEditing ? "Save changes" : "Create task";

        TitleBox.Text = TaskItem.Name;
        DescriptionBox.Text = TaskItem.Description ?? string.Empty;
        ChecklistBox.Text = string.Join(Environment.NewLine, TaskItem.Checklist.Select(item => item.Title));
        DueDatePicker.Date = TaskItem.DueDate;
        DueTimePicker.IsEnabled = TaskItem.DueDate.HasValue;
        DueTimePicker.Time = TaskItem.DueDate is DateTimeOffset existingDueDate
            && TaskItem.HasExplicitDueTime
            ? existingDueDate.TimeOfDay
            : TimeSpan.FromHours(9);
        RemindersEnabledBox.IsEnabled = TaskItem.DueDate.HasValue;
        ReminderLeadBox.IsEnabled = TaskItem.DueDate.HasValue && TaskItem.RemindersEnabled;
        SelectComboItem(CategoryBox, TaskItem.Category.ToString());
        SelectComboItem(PriorityBox, TaskItem.Priority.ToString());
        SelectComboItem(EstimatedMinutesBox, TaskItem.EstimatedMinutes.ToString());
        SelectComboItem(ReminderLeadBox, TaskItem.ReminderLeadMinutes.ToString());
        RemindersEnabledBox.IsChecked = TaskItem.RemindersEnabled;
    }

    private async void SaveButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var deferral = args.GetDeferral();
        try
        {
            var title = TitleBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                ShowValidationMessage("Enter a title for this task.");
                args.Cancel = true;
                return;
            }

            if (CategoryBox.SelectedItem is not ComboBoxItem { Tag: string categoryName }
                || !Enum.TryParse(categoryName, out Category category))
            {
                ShowValidationMessage("Choose a category for this task.");
                args.Cancel = true;
                return;
            }

            if (PriorityBox.SelectedItem is not ComboBoxItem { Tag: string priorityValue }
                || !int.TryParse(priorityValue, out var priority))
            {
                ShowValidationMessage("Choose a priority for this task.");
                args.Cancel = true;
                return;
            }

            TaskItem.Name = title;
            TaskItem.Description = DescriptionBox.Text.Trim();
            var oldChecklist = TaskItem.Checklist.ToDictionary(
                item => item.Title.Trim(),
                item => item,
                StringComparer.CurrentCultureIgnoreCase);
            TaskItem.Checklist = new ObservableCollection<TaskChecklistItem>(
                ChecklistBox.Text
                    .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => line.Trim())
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Distinct(StringComparer.CurrentCultureIgnoreCase)
                    .Select(line => oldChecklist.TryGetValue(line, out var item)
                        ? item
                        : new TaskChecklistItem { Title = line }));
            TaskItem.DueDate = DueDatePicker.Date is DateTimeOffset dueDate
                ? new DateTimeOffset(
                    dueDate.Year,
                    dueDate.Month,
                    dueDate.Day,
                    DueTimePicker.Time.Hours,
                    DueTimePicker.Time.Minutes,
                    0,
                    dueDate.Offset)
                : null;
            TaskItem.HasExplicitDueTime = TaskItem.DueDate.HasValue;
            TaskItem.Category = category;
            TaskItem.Priority = priority;
            TaskItem.RemindersEnabled = RemindersEnabledBox.IsChecked == true;

            if (EstimatedMinutesBox.SelectedItem is not ComboBoxItem { Tag: string estimateValue }
                || !int.TryParse(estimateValue, out var estimatedMinutes))
            {
                ShowValidationMessage("Choose an estimated focus time.");
                args.Cancel = true;
                return;
            }

            if (ReminderLeadBox.SelectedItem is not ComboBoxItem { Tag: string leadValue }
                || !int.TryParse(leadValue, out var reminderLeadMinutes))
            {
                ShowValidationMessage("Choose when you want the reminder.");
                args.Cancel = true;
                return;
            }

            TaskItem.EstimatedMinutes = estimatedMinutes;
            TaskItem.ReminderLeadMinutes = reminderLeadMinutes;

            if (_isEditing)
            {
                await _taskStorage.UpdateTaskAsync(TaskItem);
            }
            else
            {
                await _taskStorage.SaveTaskAsync(TaskItem);
            }
        }
        catch (Exception ex)
        {
            ShowValidationMessage($"The task could not be saved. {ex.Message}");
            args.Cancel = true;
        }
        finally
        {
            deferral.Complete();
        }
    }

    private void DueDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
    {
        DueTimePicker.IsEnabled = args.NewDate.HasValue;
        RemindersEnabledBox.IsEnabled = args.NewDate.HasValue;
        ReminderLeadBox.IsEnabled = args.NewDate.HasValue && RemindersEnabledBox.IsChecked == true;
    }

    private void RemindersEnabledBox_Click(object sender, RoutedEventArgs e)
    {
        ReminderLeadBox.IsEnabled = DueDatePicker.Date.HasValue && RemindersEnabledBox.IsChecked == true;
    }

    private static void SelectComboItem(ComboBox comboBox, string tag)
    {
        foreach (var item in comboBox.Items)
        {
            if (item is ComboBoxItem comboBoxItem && comboBoxItem.Tag?.ToString() == tag)
            {
                comboBox.SelectedItem = comboBoxItem;
                return;
            }
        }
    }

    private void ShowValidationMessage(string message)
    {
        ValidationMessage.Text = message;
        ValidationMessage.Visibility = Visibility.Visible;
    }
}

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using StudyFlow.Bus.Models;
using StudyFlow.Bus.Services;
using StudyFlow.Models;
using StudyFlow.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using StudyFlow.Views.Dialogs;

namespace StudyFlow.Views.Pages;
public sealed partial class MainPage : Page
{
    private DialogService _dialogService { get; } = new();
    private AllTasks AllTasks { get; } = new AllTasks();
    private ObservableCollection<TaskItem> Tasks { get; } = new();
    private TaskStorageService _taskStorage { get; } = new();
    public MainPage()
    {
        InitializeComponent();
    }

    private async void AppBarButton_Click(object sender, RoutedEventArgs e)
    {
        await _dialogService.ShowDialogAsync(new TaskDialog(), XamlRoot);
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        var task = (TaskItem?)(sender as Button)?.DataContext;
        if (task != null)
        {
            Tasks.Remove(task);
            _taskStorage.DeleteTask(task.ID);
        }
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        var task = (TaskItem?)(sender as Button)?.DataContext;
        if (task != null)
        {
            Frame.Navigate(typeof(AddTaskPage), task);
        }
    }

    private void MarkCompleted_Invoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
    {
        if(args.SwipeControl.DataContext is TaskItem task)
        {
            task.IsCompleted = !task.IsCompleted;
            _taskStorage.UpdateTask(task);
        }
    }
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        try
        {
            Tasks.Clear();
            var tasks = await AllTasks.GetTasksAsync();
            foreach (var task in tasks)
            {
                Tasks.Add(task);
            }  
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., log the error, show a message to the user, etc.)
            Debug.WriteLine($"Error loading tasks: {ex.Message}");
        }
    }

    private void DeleteSwipeItem_Invoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
    {
        if(args.SwipeControl.DataContext is TaskItem task)
        {
            Tasks.Remove(task);
            _taskStorage.DeleteTask(task.ID);
        }
    }

    private void EditSwipeItem_Invoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
    {               
        if(args.SwipeControl.DataContext is TaskItem task)
        {
            Frame.Navigate(typeof(AddTaskPage), task);
        }
    }

    private void ToggleButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton btn
            && btn.DataContext is TaskItem task)
        {
            task.IsCompleted = !task.IsCompleted;
            _taskStorage.UpdateTask(task);
        }
    }
}

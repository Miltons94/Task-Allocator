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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace StudyFlow.Views.Pages;
public sealed partial class MainPage : Page
{
    private AllTasks AllTasks { get; } = new AllTasks();
    private ObservableCollection<Models.TaskItem> Tasks { get; } = new();
    private TaskStorageService _taskStorage { get; } = new();
    public MainPage()
    {
        InitializeComponent();
    }

    private void AppBarButton_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(AddTaskPage));
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        var task = (Models.TaskItem?)(sender as Button)?.DataContext;
        if (task != null)
        {
            Tasks.Remove(task);
            _taskStorage.DeleteTask(task.ID);
        }
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        var task = (Models.TaskItem?)(sender as Button)?.DataContext;
        if (task != null)
        {
            Frame.Navigate(typeof(AddTaskPage), task);
        }
    }

    private void MarkCompleted_Invoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
    {
        if(args.SwipeControl.DataContext is Models.TaskItem task)
        {
            task.IsCompleted = true;
            _taskStorage.UpdateTask(task);
        }
    }
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        try
        {
            var tasks = await AllTasks.GetTasksAsync();
            Tasks.Clear();
            foreach (var task in tasks)
                Tasks.Add(task);
            
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., log the error, show a message to the user, etc.)
            Debug.WriteLine($"Error loading tasks: {ex.Message}");
        }
    }

    private void DeleteSwipeItem_Invoked(Microsoft.UI.Xaml.Controls.SwipeItem sender, SwipeItemInvokedEventArgs args)
    {
        if(args.SwipeControl.DataContext is Models.TaskItem task)
        {
            Tasks.Remove(task);
            _taskStorage.DeleteTask(task.ID);
        }
    }

    private void EditSwipeItem_Invoked(Microsoft.UI.Xaml.Controls.SwipeItem sender, SwipeItemInvokedEventArgs args)
    {               
        if(args.SwipeControl.DataContext is Models.TaskItem task)
        {
            Frame.Navigate(typeof(AddTaskPage), task);
        }
    }
}

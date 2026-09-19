using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using StudyFlow.Bus.Models;
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
    private ObservableCollection<TaskItem> Tasks { get; } = new();
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
        var task = (TaskItem?)(sender as Button)?.DataContext;
        if (task != null)
        {
            Tasks.Remove(task);
            task.Delete();
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

    private void DeleteSwipeItem_Invoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
    {
        if(args.SwipeControl.DataContext is TaskItem task)
        {
            Tasks.Remove(task);
            task.Delete();
        }
    }

    private void EditSwipeItem_Invoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
    {               
        if(args.SwipeControl.DataContext is TaskItem task)
        {
            Frame.Navigate(typeof(AddTaskPage), task);
        }
    }
}

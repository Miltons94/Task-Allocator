using System.Text.Json;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using StudyFlow.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Storage;
using Microsoft.UI.Xaml.Navigation;

namespace StudyFlow.Views.Pages;
public sealed partial class AddTaskPage : Page
{
    private string Title { get; set; } = string.Empty;
    private string Description { get; set; } = string.Empty;
    private bool IsCompleted { get; set; }
    public AddTaskPage()
    {
        InitializeComponent();
    }

    private async void AddTaskButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(Title))
        {
            Debug.WriteLine("Title is required.");
            return;
        }

        var newTask = new TaskItem
        {
            Name = Title,
            Description = Description
        };
        Debug.WriteLine("DEBUG: " + newTask.ToString());
        newTask.Save();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is TaskItem task)
        {
            Title = task.Name;
            Description = task.Description ?? string.Empty;
            IsCompleted = task.IsCompleted;
        }
    }
}

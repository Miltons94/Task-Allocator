using System.Text.Json;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using StudyFlow.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Storage;
using Microsoft.UI.Xaml.Navigation;
using StudyFlow.Bus.Services;

namespace StudyFlow.Views.Pages;
public sealed partial class AddTaskPage : Page
{
    public TaskItem TaskItem { get; set; } = new();
    private TaskStorageService _taskStorage { get; } = new();
    private bool _isEditing { get; set; } = false;
    public AddTaskPage()
    {
        InitializeComponent();
    }

    private async void AddTaskButton_Click(object sender, RoutedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(TaskItem, nameof(TaskItem));
        _taskStorage.SaveTask(TaskItem);
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is TaskItem task)
        {
            TaskItem = task;
            _isEditing = true;
        }
        else
        {
            TaskItem = new();
            _isEditing = false;
        }
    }
}

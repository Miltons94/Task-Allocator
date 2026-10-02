using StudyFlow.Enums;
using StudyFlow.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace StudyFlow.ViewModels;

public sealed class TaskViewModel
{
    public TaskItem Task { get; }
    public int AllocationScore { get; }
    public string AllocationGroup { get; }
    public string AllocationReason { get; }
    public string AllocationLabel => $"{AllocationGroup} · {AllocationReason}";
    public string CategoryLabel => Task.Category switch
    {
        Category.UniversityWork => "University work",
        Category.Personal => "Personal development",
        Category.Projects => "Projects",
        Category.Programming => "Programming",
        _ => Task.Category.ToString()
    };
    public string FocusSummary => Task.FocusMinutes > 0
        ? $"Focused {Task.FocusMinutes} min"
        : $"Estimate {Task.EstimatedMinutes} min";
    public int ChecklistCompletedCount => Task.Checklist.Count(item => item.IsCompleted);
    public int ChecklistCount => Task.Checklist.Count;
    public string ChecklistSummary => $"Subtasks · {ChecklistCompletedCount}/{ChecklistCount}";

    public TaskViewModel(TaskItem task, DateTimeOffset now)
    {
        Task = task;
        var score = 0;
        var reasons = new List<string>();

        var priorityScore = task.Priority switch
        {
            >= 3 => 35,
            2 => 20,
            1 => 8,
            _ => 0
        };
        score += priorityScore;
        if (priorityScore > 0)
        {
            reasons.Add(task.Priority >= 3 ? "High priority" : task.Priority == 2 ? "Normal priority" : "Low priority");
        }

        if (task.DueDate is DateTimeOffset dueDate)
        {
            var daysUntilDue = (dueDate.Date - now.Date).Days;
            if (daysUntilDue < 0)
            {
                score += Math.Min(60, 32 + Math.Abs(daysUntilDue) * 4);
                reasons.Add("Overdue");
            }
            else if (daysUntilDue == 0)
            {
                score += 42;
                reasons.Add("Due today");
            }
            else if (daysUntilDue == 1)
            {
                score += 32;
                reasons.Add("Due tomorrow");
            }
            else if (daysUntilDue <= 3)
            {
                score += 22;
                reasons.Add("Due soon");
            }
            else if (daysUntilDue <= 7)
            {
                score += 12;
                reasons.Add("Due this week");
            }
        }

        var categoryScore = task.Category switch
        {
            Category.UniversityWork => 4,
            Category.Projects => 3,
            Category.Programming => 2,
            _ => 0
        };
        score += categoryScore;
        reasons.Add(CategoryLabel);

        var ageDays = Math.Max(0, (now.Date - task.StartDate.Date).Days);
        var ageScore = Math.Min(10, ageDays / 2);
        score += ageScore;
        if (ageScore >= 4)
        {
            reasons.Add("Waiting a while");
        }

        if (task.EstimatedMinutes <= 25)
        {
            score += 4;
            reasons.Add("Quick win");
        }

        // Rotate attention toward work that has not already consumed several focus blocks.
        score -= Math.Min(8, task.FocusMinutes / 30);

        AllocationScore = score;
        AllocationGroup = score >= 65 ? "Focus now" : score >= 35 ? "Up next" : "Plan later";
        AllocationReason = reasons.Count == 0
            ? "No deadline set · ready when you are"
            : string.Join(" · ", reasons.Take(3));
    }

    public static List<TaskViewModel> Allocate(IEnumerable<TaskItem> tasks, DateTimeOffset? currentTime = null)
    {
        var now = currentTime ?? DateTimeOffset.Now;
        return tasks
            .Where(task => !task.IsCompleted)
            .Select(task => new TaskViewModel(task, now))
            .OrderByDescending(viewModel => viewModel.AllocationScore)
            .ThenBy(viewModel => viewModel.Task.DueDate ?? DateTimeOffset.MaxValue)
            .ThenBy(viewModel => viewModel.Task.StartDate)
            .ToList();
    }
}

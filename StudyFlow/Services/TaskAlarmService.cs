using StudyFlow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace StudyFlow.Services;

public static class TaskAlarmService
{
    private const string NotificationGroup = "StudyFlowTasks";

    public static void Reconcile(IEnumerable<TaskItem> tasks)
    {
        var notifier = ToastNotificationManager.CreateToastNotifier();

        foreach (var scheduled in notifier.GetScheduledToastNotifications().ToList())
        {
            if (scheduled.Group == NotificationGroup)
            {
                notifier.RemoveFromSchedule(scheduled);
            }
        }

        var now = DateTimeOffset.Now;
        foreach (var task in tasks.Where(task =>
                     !task.IsCompleted && task.Priority >= 3 && task.DueDate.HasValue && task.RemindersEnabled))
        {
            var dueAt = GetDueDateTime(task);
            var tagPrefix = CreateTagPrefix(task.ID);
            var reminderAt = dueAt - TimeSpan.FromMinutes(task.ReminderLeadMinutes);

            if (task.ReminderLeadMinutes > 0 && reminderAt > now)
            {
                Schedule(notifier, task, reminderAt, $"{tagPrefix}R", isAlarm: false, dueAt, task.ReminderLeadMinutes);
            }

            if (dueAt > now)
            {
                Schedule(notifier, task, dueAt, $"{tagPrefix}A", isAlarm: true, dueAt, task.ReminderLeadMinutes);
            }
        }
    }

    private static DateTimeOffset GetDueDateTime(TaskItem task)
    {
        // Tasks created before due-time support were stored at midnight; treat those as 9 AM.
        var dueDate = task.DueDate!.Value;
        return !task.HasExplicitDueTime
            ? new DateTimeOffset(dueDate.Year, dueDate.Month, dueDate.Day, 9, 0, 0, dueDate.Offset)
            : dueDate;
    }

    private static void Schedule(
        ToastNotifier notifier,
        TaskItem task,
        DateTimeOffset deliveryTime,
        string tag,
        bool isAlarm,
        DateTimeOffset dueAt,
        int reminderLeadMinutes)
    {
        if (deliveryTime <= DateTimeOffset.Now.AddSeconds(5))
        {
            return;
        }

        var title = SecurityElement.Escape(task.Name) ?? string.Empty;
        var detail = isAlarm
            ? $"This high-priority task is due now ({dueAt.ToString("h:mm tt")})."
            : $"This high-priority task is due in {FormatLeadTime(reminderLeadMinutes)} ({dueAt.ToString("h:mm tt")}).";
        var escapedDetail = SecurityElement.Escape(detail) ?? string.Empty;
        var scenario = isAlarm ? "alarm" : "reminder";
        var actions = isAlarm
            ? "<action content='Snooze' arguments='snooze' activationType='system'/>"
              + "<action content='Dismiss' arguments='dismiss' activationType='system'/>"
            : "<action content='Dismiss' arguments='dismiss' activationType='system'/>";

        var xml = $"""
            <toast scenario='{scenario}'>
              <visual>
                <binding template='ToastGeneric'>
                  <text>{(isAlarm ? "Task alarm" : "Task reminder")}</text>
                  <text>{title}</text>
                  <text>{escapedDetail}</text>
                </binding>
              </visual>
              <actions>{actions}</actions>
            </toast>
            """;

        var content = new XmlDocument();
        content.LoadXml(xml);
        var scheduled = new ScheduledToastNotification(content, deliveryTime)
        {
            Group = NotificationGroup,
            Tag = tag
        };
        notifier.AddToSchedule(scheduled);
    }

    private static string FormatLeadTime(int minutes)
        => minutes switch
        {
            60 => "1 hour",
            1440 => "1 day",
            _ => $"{minutes} minutes"
        };

    private static string CreateTagPrefix(Guid taskId)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(taskId.ToString("N")));
        return Convert.ToHexString(bytes.AsSpan(0, 7));
    }
}

using System.Text.Json;
using StudyFlow.Bus.Enums;
using StudyFlow.Bus.Helpers;
using StudyFlow.Bus.Interfaces;
using StudyFlow.Bus.Models;

namespace StudyFlow.Bus.Services;

public sealed class AcademicEventService : IAcademicEventService
{
    private static readonly SemaphoreSlim FileLock = new(1, 1);
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    public async Task<IReadOnlyList<AcademicEvent>> GetEventsAsync(
        CancellationToken cancellationToken = default)
    {
        await FileLock.WaitAsync(cancellationToken);
        try
        {
            var events = await ReadEventsAsync(cancellationToken);
            return events
                .OrderBy(academicEvent => academicEvent.StartDate)
                .ThenBy(academicEvent => academicEvent.Title, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }
        finally
        {
            FileLock.Release();
        }
    }

    public async Task SaveEventAsync(
        AcademicEvent academicEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(academicEvent);
        Validate(academicEvent);

        await FileLock.WaitAsync(cancellationToken);
        try
        {
            var events = await ReadEventsAsync(cancellationToken);
            var existingIndex = events.FindIndex(item => item.Id == academicEvent.Id);
            if (existingIndex >= 0)
            {
                events[existingIndex] = academicEvent;
            }
            else
            {
                events.Add(academicEvent);
            }

            await WriteEventsAsync(events, cancellationToken);
        }
        finally
        {
            FileLock.Release();
        }
    }

    public async Task DeleteEventAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await FileLock.WaitAsync(cancellationToken);
        try
        {
            var events = await ReadEventsAsync(cancellationToken);
            if (events.RemoveAll(academicEvent => academicEvent.Id == id) > 0)
            {
                await WriteEventsAsync(events, cancellationToken);
            }
        }
        finally
        {
            FileLock.Release();
        }
    }

    private static void Validate(AcademicEvent academicEvent)
    {
        if (string.IsNullOrWhiteSpace(academicEvent.Title))
        {
            throw new ArgumentException("An event title is required.", nameof(academicEvent));
        }

        if (!Enum.IsDefined(academicEvent.Type))
        {
            throw new ArgumentOutOfRangeException(nameof(academicEvent), "The event type is invalid.");
        }

        if (academicEvent.EndDate is DateTimeOffset endDate
            && endDate.Date < academicEvent.StartDate.Date)
        {
            throw new ArgumentException("The end date cannot be before the start date.", nameof(academicEvent));
        }

        academicEvent.Title = academicEvent.Title.Trim();
        academicEvent.Description = string.IsNullOrWhiteSpace(academicEvent.Description)
            ? null
            : academicEvent.Description.Trim();
        academicEvent.Location = string.IsNullOrWhiteSpace(academicEvent.Location)
            ? null
            : academicEvent.Location.Trim();
    }

    private static async Task<List<AcademicEvent>> ReadEventsAsync(CancellationToken cancellationToken)
    {
        var filePath = AcademicEventStoragePath.GetStorageFilePath();
        if (!File.Exists(filePath))
        {
            return [];
        }

        var json = await File.ReadAllTextAsync(filePath, cancellationToken);
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<AcademicEvent>>(json) ?? [];
    }

    private static async Task WriteEventsAsync(
        List<AcademicEvent> events,
        CancellationToken cancellationToken)
    {
        var filePath = AcademicEventStoragePath.GetStorageFilePath();
        var temporaryPath = $"{filePath}.tmp";
        var json = JsonSerializer.Serialize(events, SerializerOptions);
        await File.WriteAllTextAsync(temporaryPath, json, cancellationToken);
        File.Move(temporaryPath, filePath, overwrite: true);
    }
}

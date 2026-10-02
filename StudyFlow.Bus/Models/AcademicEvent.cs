using StudyFlow.Bus.Enums;
using System.Globalization;
using System.Text.Json.Serialization;

namespace StudyFlow.Bus.Models;

public class AcademicEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public AcademicEventType Type { get; set; } = AcademicEventType.Other;
    public DateTimeOffset StartDate { get; set; } = DateTimeOffset.Now.Date;
    public DateTimeOffset? EndDate { get; set; }

    [JsonIgnore]
    public string MonthLabel => StartDate.ToString("MMM", CultureInfo.CurrentCulture).ToUpper(CultureInfo.CurrentCulture);

    [JsonIgnore]
    public string DayLabel => StartDate.ToString("dd", CultureInfo.CurrentCulture);

    [JsonIgnore]
    public string TypeLabel => Type.ToString();

    [JsonIgnore]
    public string DateLabel => EndDate is DateTimeOffset endDate && endDate.Date > StartDate.Date
        ? $"{StartDate.ToString("MMM d", CultureInfo.CurrentCulture)} - {endDate.ToString("MMM d", CultureInfo.CurrentCulture)}"
        : StartDate.ToString("ddd, MMM d", CultureInfo.CurrentCulture);
}

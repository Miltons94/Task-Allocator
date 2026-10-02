using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using StudyFlow.Bus.Enums;
using StudyFlow.Bus.Models;

namespace StudyFlow.Views.Dialogs;

public sealed partial class AcademicEventDialog : ContentDialog
{
    public AcademicEvent Event { get; }

    public AcademicEventDialog(AcademicEvent? academicEvent = null)
    {
        InitializeComponent();
        Event = academicEvent is null
            ? new AcademicEvent()
            : new AcademicEvent
            {
                Id = academicEvent.Id,
                Title = academicEvent.Title,
                Description = academicEvent.Description,
                Location = academicEvent.Location,
                Type = academicEvent.Type,
                StartDate = academicEvent.StartDate,
                EndDate = academicEvent.EndDate
            };

        Title = academicEvent is null ? "Create event" : "Edit event";
        TitleBox.Text = Event.Title;
        DescriptionBox.Text = Event.Description ?? string.Empty;
        LocationBox.Text = Event.Location ?? string.Empty;
        StartDatePicker.Date = Event.StartDate;
        EndDatePicker.Date = Event.EndDate;
        TypeBox.SelectedIndex = (int)Event.Type;
    }

    private void SaveButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var title = TitleBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            ShowValidationMessage("Enter a name for this event.");
            args.Cancel = true;
            return;
        }

        if (StartDatePicker.Date is not DateTimeOffset startDate)
        {
            ShowValidationMessage("Choose a start date for this event.");
            args.Cancel = true;
            return;
        }

        if (EndDatePicker.Date is DateTimeOffset endDate
            && endDate.Date < startDate.Date)
        {
            ShowValidationMessage("The end date must be on or after the start date.");
            args.Cancel = true;
            return;
        }

        if (TypeBox.SelectedItem is not ComboBoxItem { Tag: string typeName }
            || !Enum.TryParse(typeName, out AcademicEventType eventType))
        {
            ShowValidationMessage("Choose a valid event type.");
            args.Cancel = true;
            return;
        }

        Event.Title = title;
        Event.Description = DescriptionBox.Text;
        Event.Location = LocationBox.Text;
        Event.Type = eventType;
        Event.StartDate = startDate;
        Event.EndDate = EndDatePicker.Date;
    }

    private void ShowValidationMessage(string message)
    {
        ValidationMessage.Text = message;
        ValidationMessage.Visibility = Visibility.Visible;
    }
}

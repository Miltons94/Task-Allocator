using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using Windows.UI.Text;

namespace StudyFlow.Converters;

public sealed class DueDateForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var resourceKey = value is true
            ? "SystemFillColorCriticalBrush"
            : "AccentTextFillColorPrimaryBrush";

        return Application.Current.Resources.TryGetValue(resourceKey, out var brush)
            ? brush
            : Application.Current.Resources["TextFillColorPrimaryBrush"];
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}

public sealed class OptionalDateVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => value is DateTimeOffset ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}

public sealed class OptionalTextVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => value is string text && !string.IsNullOrWhiteSpace(text)
            ? Visibility.Visible
            : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}

public sealed class CompletedTaskTextDecorationsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => value is true ? TextDecorations.Strikethrough : TextDecorations.None;

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}

public sealed class NonZeroVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => value is int count && count > 0 ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}

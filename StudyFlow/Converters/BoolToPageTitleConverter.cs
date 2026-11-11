using Microsoft.UI.Xaml.Data;
using System;

namespace StudyFlow.Converters;

public class BoolToPageTitleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if(value is bool isEdit)
        {
            return isEdit == true ? "Edit Task" : "Create Task";
        }
        return "Create Task"; 
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

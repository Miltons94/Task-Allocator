using Microsoft.UI.Xaml.Data;
using System;


namespace StudyFlow.Converters;
public class BoolToButtonTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if(value is bool isEdit)
        {
            return  isEdit? "Save changes" : "Save";
        }
        return "Save"; 
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

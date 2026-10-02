using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using StudyFlow.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyFlow.Converters;

public class CollectionVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if(parameter is string param && !string.IsNullOrEmpty(param))
        {
            if(param.Equals("Invert", StringComparison.OrdinalIgnoreCase))
            {
                return (int)value > 0
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }
        return (int)value > 0
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

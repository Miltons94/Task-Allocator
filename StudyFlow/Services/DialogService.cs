using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyFlow.Services;
public class DialogService
{
    public async Task<ContentDialogResult> ShowDialogAsync(ContentDialog dialog, XamlRoot xamlRoot)
    {
        if (xamlRoot is null)
            throw new ArgumentNullException(
                nameof(xamlRoot), "Content dialog depends on XamlRoot element!");

        dialog.XamlRoot = xamlRoot;
        var res = await dialog.ShowAsync();
        return res;
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyFlow.Bus.Helpers;
public static class TaskStoragePath
{
    private readonly static string _fileName = "tasks.json";
    private readonly static string _appName = "StudyFlow";
    private static string _folderPath = Environment.GetFolderPath(
        Environment.SpecialFolder.ApplicationData);

    public static string GetStorageFilePath()
    {
        var storageFolder = Path.Combine(_folderPath, _appName);
        if (!Directory.Exists(storageFolder))
        {
            Directory.CreateDirectory(storageFolder);
        }
        var filePath = Path.Combine(storageFolder, _fileName);
        if(!File.Exists(filePath))
        {
            File.Create(filePath).Dispose();
        }
        return filePath;
    }
}

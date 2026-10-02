namespace StudyFlow.Bus.Helpers;

public static class AcademicEventStoragePath
{
    public static string GetStorageFilePath()
    {
        var storageFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "StudyFlow");
        Directory.CreateDirectory(storageFolder);
        return Path.Combine(storageFolder, "academic-events.json");
    }
}

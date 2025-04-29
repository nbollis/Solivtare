namespace SolvitaireIO;

public static class ClassExtensions
{
    public static string GetUniqueFilePath(this string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        string directory = Path.GetDirectoryName(filePath) ?? string.Empty;
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        string extension = Path.GetExtension(filePath);

        string uniqueFilePath = filePath;
        int counter = 1;

        while (File.Exists(uniqueFilePath))
        {
            uniqueFilePath = Path.Combine(directory, $"{fileNameWithoutExtension} ({counter}){extension}");
            counter++;
        }

        return uniqueFilePath;
    }
}
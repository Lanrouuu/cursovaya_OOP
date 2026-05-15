namespace Cursovaya.Services;

using System.IO;

public class ImageService
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"];

    public string CopyToUserImages(string sourcePath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
        {
            return string.Empty;
        }

        var extension = Path.GetExtension(sourcePath).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            return string.Empty;
        }

        var imagesDirectory = Path.Combine(AppContext.BaseDirectory, "Assets", "UserImages");
        Directory.CreateDirectory(imagesDirectory);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var destinationPath = Path.Combine(imagesDirectory, fileName);
        File.Copy(sourcePath, destinationPath);

        return destinationPath;
    }

    public static bool IsAllowedImageExtension(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return AllowedExtensions.Contains(extension);
    }
}

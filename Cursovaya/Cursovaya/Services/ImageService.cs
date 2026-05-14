namespace Cursovaya.Services;

using System.IO;

public class ImageService
{
    public string CopyToUserImages(string sourcePath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
        {
            return string.Empty;
        }

        var imagesDirectory = Path.Combine(AppContext.BaseDirectory, "Assets", "UserImages");
        Directory.CreateDirectory(imagesDirectory);

        var extension = Path.GetExtension(sourcePath);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var destinationPath = Path.Combine(imagesDirectory, fileName);
        File.Copy(sourcePath, destinationPath);

        return destinationPath;
    }
}

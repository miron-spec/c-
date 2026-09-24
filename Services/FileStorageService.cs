using Microsoft.AspNetCore.Http;

namespace MatchApp.Backend.Services;

public class FileStorageService(IWebHostEnvironment environment, IConfiguration configuration)
{
    private readonly string[] imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
    private readonly string[] videoExtensions = new[] { ".mp4", ".webm", ".mov" };

    public async Task<string> SaveAsync(IFormFile file, string category, CancellationToken ct)
    {
        if (file.Length <= 0) throw new InvalidOperationException("Empty file.");
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var isImage = imageExtensions.Contains(ext);
        var isVideo = videoExtensions.Contains(ext);
        if (!isImage && !isVideo) throw new InvalidOperationException("Unsupported file type.");

        var max = isImage
            ? configuration.GetValue<long>("FileStorage:MaxImageBytes", 10_485_760)
            : configuration.GetValue<long>("FileStorage:MaxVideoBytes", 52_428_800);
        if (file.Length > max) throw new InvalidOperationException("File is too large.");

        var root = configuration["FileStorage:Root"] ?? "wwwroot/uploads";
        var absolute = Path.Combine(environment.ContentRootPath, root, category);
        Directory.CreateDirectory(absolute);
        var name = $"{Guid.NewGuid():N}{ext}";
        var path = Path.Combine(absolute, name);
        await using var stream = File.Create(path);
        await file.CopyToAsync(stream, ct);
        return $"/uploads/{category}/{name}";
    }
}

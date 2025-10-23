using Zhg.FlowForge.App.Shared.Interfaces;

namespace Zhg.FlowForge.App.Shared.Services;


public class FileSystemService : IFileSystemService
{
    public string GetAppDataDirectory()
    {
        // 跨平台应用数据目录
#if ANDROID
            return FileSystem.AppDataDirectory;
#elif IOS || MACCATALYST
            return FileSystem.AppDataDirectory;
#elif WINDOWS
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
#else
        // Blazor Server / WebAssembly
        return AppContext.BaseDirectory;
#endif
    }

    public string CombinePaths(params string[] paths)
    {
        return Path.Combine(paths);
    }

    public async Task<bool> FileExistsAsync(string path)
    {
        try
        {
            return File.Exists(path);
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> ReadFileAsync(string path)
    {
        return await File.ReadAllTextAsync(path);
    }

    public async Task WriteFileAsync(string path, string content)
    {
        // 确保目录存在
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(path, content);
    }

    public async Task<bool> DirectoryExistsAsync(string path)
    {
        return Directory.Exists(path);
    }

    public async Task CreateDirectoryAsync(string path)
    {
        Directory.CreateDirectory(path);
    }

    public async Task<List<string>> GetFilesAsync(string directory, string searchPattern = "*.*")
    {
        if (!Directory.Exists(directory))
            return new List<string>();

        return Directory.GetFiles(directory, searchPattern).ToList();
    }
}

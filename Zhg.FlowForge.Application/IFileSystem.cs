namespace Zhg.FlowForge.Domain;

public interface IFileSystem
{
    Task<bool> FileExistsAsync(string path);
    Task<string> ReadFileAsync(string path);
    Task WriteFileAsync(string path, string content);
    Task<bool> DirectoryExistsAsync(string path);
    Task CreateDirectoryAsync(string path);
    Task<List<string>> GetFilesAsync(string directory, string searchPattern = "*.*");
    string GetAppDataDirectory();
    string CombinePaths(params string[] paths);
}
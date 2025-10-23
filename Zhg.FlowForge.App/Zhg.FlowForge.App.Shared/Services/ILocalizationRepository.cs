//using System;
//using System.Collections.Generic;
//using System.Text;

//// Services/LocalizationRepository.cs
//using Microsoft.Data.Sqlite;
//using Microsoft.Extensions.Options;
//using System.Data;


//using Microsoft.Extensions.Options;

//namespace Zhg.FlowForge.App.Shared.Services;


//// Services/ILocalizationRepository.cs
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Zhg.FlowForge.App.Shared.Models;

//public interface ILocalizationRepository
//{
//    Task InitializeDatabaseAsync();
//    Task<List<LanguageResource>> GetResourcesByCultureAsync(string culture);
//    Task<Dictionary<string, string>> GetResourceDictionaryAsync(string culture);
//    Task AddOrUpdateResourceAsync(LanguageResource resource);
//    Task AddResourcesAsync(List<LanguageResource> resources);
//    Task<List<string>> GetAvailableCulturesAsync();
//}



//public class LocalizationRepository : ILocalizationRepository
//{
//    private readonly string _databasePath;
//    private readonly string _connectionString;

//    public LocalizationRepository(IOptions<AppSettings> settings)
//    {
//        _databasePath = Path.Combine(FileSystem.AppDataDirectory, "localization.db");
//        _connectionString = $"Data Source={_databasePath}";
//    }

//    public async Task InitializeDatabaseAsync()
//    {
//        using var connection = new SqliteConnection(_connectionString);
//        await connection.OpenAsync();

//        var command = connection.CreateCommand();
//        command.CommandText = @"
//            CREATE TABLE IF NOT EXISTS LanguageResources (
//                Id INTEGER PRIMARY KEY AUTOINCREMENT,
//                Key TEXT NOT NULL,
//                Value TEXT NOT NULL,
//                Culture TEXT NOT NULL,
//                Module TEXT NOT NULL,
//                CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
//                UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
//                UNIQUE(Key, Culture)
//            );

//            CREATE INDEX IF NOT EXISTS IX_LanguageResources_Culture ON LanguageResources(Culture);
//            CREATE INDEX IF NOT EXISTS IX_LanguageResources_Key ON LanguageResources(Key);
//        ";

//        await command.ExecuteNonQueryAsync();
//    }

//    public async Task<List<LanguageResource>> GetResourcesByCultureAsync(string culture)
//    {
//        var resources = new List<LanguageResource>();

//        using var connection = new SqliteConnection(_connectionString);
//        await connection.OpenAsync();

//        var command = connection.CreateCommand();
//        command.CommandText = "SELECT Key, Value, Culture, Module FROM LanguageResources WHERE Culture = @Culture";
//        command.Parameters.AddWithValue("@Culture", culture);

//        using var reader = await command.ExecuteReaderAsync();
//        while (await reader.ReadAsync())
//        {
//            resources.Add(new LanguageResource
//            {
//                Key = reader.GetString("Key"),
//                Value = reader.GetString("Value"),
//                Culture = reader.GetString("Culture"),
//                Module = reader.GetString("Module")
//            });
//        }

//        return resources;
//    }

//    public async Task<Dictionary<string, string>> GetResourceDictionaryAsync(string culture)
//    {
//        var dictionary = new Dictionary<string, string>();
//        var resources = await GetResourcesByCultureAsync(culture);

//        foreach (var resource in resources)
//        {
//            dictionary[resource.Key] = resource.Value;
//        }

//        return dictionary;
//    }

//    public async Task AddOrUpdateResourceAsync(LanguageResource resource)
//    {
//        using var connection = new SqliteConnection(_connectionString);
//        await connection.OpenAsync();

//        var command = connection.CreateCommand();
//        command.CommandText = @"
//            INSERT OR REPLACE INTO LanguageResources (Key, Value, Culture, Module, UpdatedAt)
//            VALUES (@Key, @Value, @Culture, @Module, CURRENT_TIMESTAMP)
//        ";

//        command.Parameters.AddWithValue("@Key", resource.Key);
//        command.Parameters.AddWithValue("@Value", resource.Value);
//        command.Parameters.AddWithValue("@Culture", resource.Culture);
//        command.Parameters.AddWithValue("@Module", resource.Module);

//        await command.ExecuteNonQueryAsync();
//    }

//    public async Task AddResourcesAsync(List<LanguageResource> resources)
//    {
//        using var connection = new SqliteConnection(_connectionString);
//        await connection.OpenAsync();

//        using var transaction = await connection.BeginTransactionAsync();

//        try
//        {
//            foreach (var resource in resources)
//            {
//                var command = connection.CreateCommand();
//                command.CommandText = @"
//                    INSERT OR REPLACE INTO LanguageResources (Key, Value, Culture, Module, UpdatedAt)
//                    VALUES (@Key, @Value, @Culture, @Module, CURRENT_TIMESTAMP)
//                ";

//                command.Parameters.AddWithValue("@Key", resource.Key);
//                command.Parameters.AddWithValue("@Value", resource.Value);
//                command.Parameters.AddWithValue("@Culture", resource.Culture);
//                command.Parameters.AddWithValue("@Module", resource.Module);

//                await command.ExecuteNonQueryAsync();
//            }

//            await transaction.CommitAsync();
//        }
//        catch
//        {
//            await transaction.RollbackAsync();
//            throw;
//        }
//    }

//    public async Task<List<string>> GetAvailableCulturesAsync()
//    {
//        var cultures = new List<string>();

//        using var connection = new SqliteConnection(_connectionString);
//        await connection.OpenAsync();

//        var command = connection.CreateCommand();
//        command.CommandText = "SELECT DISTINCT Culture FROM LanguageResources";

//        using var reader = await command.ExecuteReaderAsync();
//        while (await reader.ReadAsync())
//        {
//            cultures.Add(reader.GetString("Culture"));
//        }

//        return cultures;
//    }
//}
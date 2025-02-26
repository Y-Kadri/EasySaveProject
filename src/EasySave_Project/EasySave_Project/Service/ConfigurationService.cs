using System.IO;

namespace EasySave_Project.Service;

using Microsoft.Extensions.Configuration;
using System;

public sealed class ConfigurationService
{
    private static readonly object _lock = new object();
    
    private static ConfigurationService _instance;

    private IConfigurationRoot configuration;

    private ConfigurationService()
    {
        string resourceFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resource");
        
        configuration = new ConfigurationBuilder()
            .SetBasePath(resourceFolder)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
    }
    
    /// <summary>
    /// Retrieves the singleton instance of ConfigurationService, ensuring thread safety.
    /// </summary>
    /// <returns>The single instance of ConfigurationService.</returns>
    public static ConfigurationService GetInstance()
    {
        if (_instance == null)
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new ConfigurationService();
                }
            }
        }
        return _instance;
    }
    
    /// <summary>
    /// Retrieves a string configuration value from the application settings using the specified key.
    /// </summary>
    /// <param name="key">The key of the setting to retrieve.</param>
    /// <returns>The string value of the specified setting, or null if not found.</returns>
    public string? GetStringSetting(string key)
    {
        return configuration[$"AppSettings:{key}"];
    }
    
}
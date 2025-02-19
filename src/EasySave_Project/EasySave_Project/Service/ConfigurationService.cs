using System.IO;

namespace EasySave_Project.Service;

using Microsoft.Extensions.Configuration;
using System;

public sealed class ConfigurationService
{
    private static ConfigurationService _instance;

    private IConfigurationRoot configuration;

    private ConfigurationService()
    {
        string resourceFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resource");
        
        this.configuration = new ConfigurationBuilder()
            .SetBasePath(resourceFolder)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
    }
    
    public static ConfigurationService GetInstance()
    {
        if (_instance == null)
        {
            _instance = new ConfigurationService();
        }
        return _instance;
    }

    public IConfigurationRoot GetConfiguration()
    {
        return this.configuration;
    }
    
    public string? GetStringSetting(string key)
    {
        return this.configuration[$"AppSettings:{key}"];
    }
    
    public int? GetIntSetting(string key)
    {
        string value = this.configuration[$"AppSettings:{key}"];
    
        if (int.TryParse(value, out int result))
        {
            return result;
        }

        return null;
    }
}
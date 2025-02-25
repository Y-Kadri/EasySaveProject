using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using DynamicData;
using EasySave_Library_Log.manager;
using EasySave_Project.Dto;
using EasySave_Project.Model;
using EasySave_Project.Service;

namespace EasySave_Project.Util;

public static class SettingUtil
{
    private static readonly string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "easySave", "easySaveSetting");
    private static readonly string filePath = Path.Combine(directoryPath, "appSetting.json");

    /// <summary>
    /// Changes the application language setting and updates the configuration.
    /// </summary>
    /// <param name="language">The new language to set.</param>
    /// <returns>True if the language was updated successfully, otherwise false.</returns>
    public static bool SettingChangeLanguage(LanguageEnum language)
    {
        return UpdateSetting(settings => settings.language = language, "languageUpdated", "errorUpdatingLanguage");
    }

    /// <summary>
    /// Changes the log format setting and updates the configuration.
    /// </summary>
    /// <param name="format">The new log format to apply.</param>
    /// <returns>True if the format was updated successfully, otherwise false.</returns>
    public static bool SettingChangeFormat(EasySave_Library_Log.manager.LogFormatManager.LogFormat format)
    {
        return UpdateSetting(settings => settings.logFormat = format, "formatUpdated", "errorFormatLanguage");
    }
    
    /// <summary>
    /// Adds a new value to a specified settings list if it does not already exist.
    /// </summary>
    /// <param name="key">The key representing the list to update.</param>
    /// <param name="value">The value to add to the list.</param>
    /// <returns>True if the value was successfully added, otherwise false.</returns>
    public static bool AddToList(string key, string value)
    {
        var settings = GetSetting();
        if (settings == null) return false;

        switch (key)
        {
            case "EncryptedFileExtensions":
                if (!settings.EncryptedFileExtensions.Contains(value))
                    settings.EncryptedFileExtensions.Add(checkFormat(value, settings.EncryptedFileExtensions));
                break;

            case "PriorityBusinessProcess":
                if (!settings.PriorityBusinessProcess.Contains(value))
                    settings.PriorityBusinessProcess.Add(value);
                break;

            default:
                return false;
        }

        return SaveSettings(settings, "itemAdded", "errorAddingItem");
    }

    /// <summary>
    /// Adds a value to a specified list in the JobSettingsDto object.
    /// If the value is not already present, it is added to the list and the changes are saved to the JSON file.
    /// </summary>
    /// <param name="value">The value to add (e.g., ".txt", ".pdf", "notepad.exe").</param>
    public static string checkFormat(string value, List<string> list)
    { 
        if (!string.IsNullOrWhiteSpace(value) && !value.StartsWith('.'))
        {
            value = "." + value;
        }
        return value;
    }


    /// <summary>
    /// Removes a specified value from a given settings list.
    /// </summary>
    /// <param name="key">The key representing the list to update.</param>
    /// <param name="value">The value to remove from the list.</param>
    /// <returns>True if the value was successfully removed, otherwise false.</returns>
    public static bool RemoveFromList(string key, string value)
    {
        var settings = GetSetting();
        if (settings == null) return false;

        switch (key)
        {
            case "EncryptedFileExtensions":
                settings.EncryptedFileExtensions.Remove(value);
                break;

            case "PriorityBusinessProcess":
                settings.PriorityBusinessProcess.Remove(value);
                break;

            default:
                return false;
        }

        return SaveSettings(settings, "itemRemoved", "errorRemovingItem");
    }

    /// <summary>
    /// Retrieves a list of values from the specified settings key.
    /// </summary>
    /// <param name="key">The key representing the list to retrieve.</param>
    /// <returns>A list of stored values, or an empty list if the key is not found.</returns>
    public static List<string> GetList(string key)
    {
        var settings = GetSetting();
        return key switch
        {
            "EncryptedFileExtensions" => settings?.EncryptedFileExtensions ?? new List<string>(),
            "PriorityBusinessProcess" => settings?.PriorityBusinessProcess ?? new List<string>(),
            _ => new List<string>()
        };
    }

    /// <summary>
    /// Initializes the settings file if it does not already exist.
    /// </summary>
    public static void InitSetting()
    {
        if (File.Exists(filePath))
        {
            return;
        }

        EnsureDirectoryAndFileExist();

        var data = new AppSettingDto(LanguageEnum.FR, LogFormatManager.LogFormat.JSON);
        SaveSettings(data, "jsonFileCreated", "errorWritingJsonFile");
    }

    /// <summary>
    /// Retrieves the current application settings from the configuration file.
    /// </summary>
    /// <returns>An <see cref="AppSettingDto"/> object if successful, otherwise null.</returns>
    public static AppSettingDto? GetSetting()
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        try
        {
            string jsonString = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<AppSettingDto>(jsonString);
        }
        catch (Exception ex)
        {
            LogManager.Instance.AddMessage(TranslationService.GetInstance().GetText("errorReadingJsonFile") + ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Saves the application settings to the configuration file.
    /// </summary>
    /// <param name="settings">The settings object to save.</param>
    /// <param name="successMessageKey">The translation key for the success message.</param>
    /// <param name="errorMessageKey">The translation key for the error message.</param>
    /// <returns>True if the settings were saved successfully, otherwise false.</returns>
    private static bool SaveSettings(AppSettingDto settings, string successMessageKey, string errorMessageKey)
    {
        try
        {
            string jsonString = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, jsonString);

            LogManager.Instance.AddMessage(TranslationService.GetInstance().GetText(successMessageKey));
            return true;
        }
        catch (Exception ex)
        {
            LogManager.Instance.AddMessage(TranslationService.GetInstance().GetText(errorMessageKey) + ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Updates a specific setting in the configuration file.
    /// </summary>
    /// <param name="updateAction">An action to modify the settings.</param>
    /// <param name="successMessageKey">The translation key for the success message.</param>
    /// <param name="errorMessageKey">The translation key for the error message.</param>
    /// <returns>True if the update was successful, otherwise false.</returns>
    private static bool UpdateSetting(Action<AppSettingDto> updateAction, string successMessageKey, string errorMessageKey)
    {
        var settings = GetSetting();
        if (settings == null)
        {
            return false;
        }

        updateAction(settings);
        return SaveSettings(settings, successMessageKey, errorMessageKey);
    }

    /// <summary>
    /// Ensures that the settings directory and file exist.
    /// </summary>
    private static void EnsureDirectoryAndFileExist()
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "{}");
        }
    }
}


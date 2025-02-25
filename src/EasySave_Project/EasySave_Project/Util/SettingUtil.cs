using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using DynamicData;
using EasySave_Library_Log.manager;
using EasySave_Project.Dto;
using EasySave_Project.Model;
using EasySave_Project.Service;
using Tmds.DBus.Protocol;

namespace EasySave_Project.Util;

public static class SettingUtil
{

    private static readonly string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "easySave", "easySaveSetting");
    private static readonly string filePath = Path.Combine(directoryPath, "appSetting.json");
    private static string Message;

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

    public static bool AddPriorityExtension(string key, PriorityExtensionDTO value)
    {
        var settings = GetSetting();
        if (settings == null) return false;

        if (key == "PriorityExtensionFiles")
        {
            if (!settings.PriorityExtensionFiles.Any(e => e.ExtensionFile == value.ExtensionFile))
            {
                settings.PriorityExtensionFiles.Add(value);
                return SaveSettings(settings, "itemAdded", "errorAddingItem");
            }
        }

        return false;
    }

    public static bool RemovePriorityExtension(string key, string extensionFile)
    {
        var settings = GetSetting();
        if (settings == null) return false;

        if (key == "PriorityExtensionFiles")
        {
            var itemToRemove = settings.PriorityExtensionFiles
                .FirstOrDefault(e => e.ExtensionFile.Equals(extensionFile, StringComparison.OrdinalIgnoreCase));

            if (itemToRemove != null)
            {
                settings.PriorityExtensionFiles.Remove(itemToRemove);
                return SaveSettings(settings, "itemRemoved", "errorRemovingItem");
            }
        }

        return false;
    }

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

    public static List<PriorityExtensionDTO> GetPriorityExtensionFilesList(string key)
    {
        var settings = GetSetting();
        return key switch
        {
            "PriorityExtensionFiles" => settings?.PriorityExtensionFiles ?? new List<PriorityExtensionDTO>(),
            _ => new List<PriorityExtensionDTO>(),
        };
    }

    public static List<PriorityExtensionDTO> GetPriorityExtensionInOrderByIndex(string key)
    {
        // Retrieve the list of priority extensions
        List<PriorityExtensionDTO> priorityExtensionFilesp = GetPriorityExtensionFilesList(key);

        // Sort the list of extensions by the 'Index' property
        // This assumes that PriorityExtensionDTO has an 'Index' property that is used to determine priority
        var sortedList = priorityExtensionFilesp.OrderBy(p => p.Index).ToList();

        return sortedList;
    }

    public static List<string> SortFilesByPriority(List<string> filePaths)
    {
        // Retrieve the list of sorted priority extensions
        List<PriorityExtensionDTO> sortedPriorityExtensions = GetPriorityExtensionInOrderByIndex("PriorityExtensionFiles");

        // Sort the file paths based on the priority of their extensions
        var sortedFiles = filePaths.OrderBy(filePath =>
        {
            // Get the extension of the current file
            string fileExtension = Path.GetExtension(filePath).ToLower();

            // Find the priority of the extension in the sorted list
            var priority = sortedPriorityExtensions.FirstOrDefault(p => p.ExtensionFile.Equals(fileExtension, StringComparison.OrdinalIgnoreCase));

            // Return the priority index if found, or a default value (999) if the extension is not found
            return priority?.Index ?? 999;
        }).ToList();

        return sortedFiles;
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
    public static bool IsExtensionPriority(string extension)
    {
        var priorityExtensions = GetSetting()?.EncryptedFileExtensions ?? new List<string>();
        return priorityExtensions.Contains(extension);
    }

    private static AppSettingDto? LoadSettings()
    {
        return GetSetting();
    }

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
    public static void MovePriorityExtensionFileUp(int index)
    {
        var settings = GetSetting();
        if (settings == null || settings.PriorityExtensionFiles == null || index <= 0) return;

        // Trouve l'�l�ment � l'index donn�
        var item = settings.PriorityExtensionFiles.FirstOrDefault(x => x.Index == index);
        if (item == null) return;  // Si l'�l�ment n'est pas trouv�, on sort

        // Trouve l'�l�ment pr�c�dent dans la liste qui a un index plus petit
        var previousItem = settings.PriorityExtensionFiles
            .Where(x => x.Index < index)
            .OrderByDescending(x => x.Index)  // Trie par index d�croissant
            .FirstOrDefault();

        if (previousItem != null)
        {
            // �change les indices des deux objets
            int tempIndex = item.Index;
            item.Index = previousItem.Index;
            previousItem.Index = tempIndex;

            // Sauvegarde les param�tres
            SaveSettings(settings, "priorityExtensionMovedUp", "errorUpdatingPriorityExtension");
        }
    }



    public static void MovePriorityExtensionFileDown(int index)
    {
        var settings = GetSetting();
        if (settings == null || settings.PriorityExtensionFiles == null || index >= settings.PriorityExtensionFiles.Count) return;

        // Trouve l'�l�ment � l'index donn�
        var item = settings.PriorityExtensionFiles.FirstOrDefault(x => x.Index == index);
        if (item == null) return;  // Si l'�l�ment n'est pas trouv�, on sort

        // Trouve l'�l�ment suivant dans la liste qui a un index plus grand
        var nextItem = settings.PriorityExtensionFiles
            .Where(x => x.Index > index)
            .OrderBy(x => x.Index)  // Trie par index croissant
            .FirstOrDefault();

        if (nextItem != null)
        {
            // �change les indices des deux objets
            int tempIndex = item.Index;
            item.Index = nextItem.Index;
            nextItem.Index = tempIndex;

            // Sauvegarde les param�tres
            SaveSettings(settings, "priorityExtensionMovedDown", "errorUpdatingPriorityExtension");
        }
    }
    public static bool AddPriorityExtension(string Extension)
    {
        if (!Extension.StartsWith("."))
        {
          Extension = "." + Extension;
            
        }
        int NewIndex = GetPriorityExtensionFilesList("PriorityExtensionFiles").Count + 1;
        PriorityExtensionDTO NewExtensionFile = new PriorityExtensionDTO();
        NewExtensionFile.ExtensionFile = Extension;
        NewExtensionFile.Index = NewIndex;
        return AddPriorityExtension("PriorityExtensionFiles", NewExtensionFile);
    }
    public static bool RemovePriorityExtension(string Extension)
    {
        var settings = GetSetting();
        if (settings == null) return false;

        // Cherche l'�l�ment � supprimer en fonction de l'extension
        var itemToRemove = settings.PriorityExtensionFiles
            .FirstOrDefault(e => e.ExtensionFile.Equals(Extension, StringComparison.OrdinalIgnoreCase));

        if (itemToRemove != null)
        {
            // Supprimer l'�l�ment trouv�
            settings.PriorityExtensionFiles.Remove(itemToRemove);

            // R�ajuster les indices des �l�ments dont l'index est plus grand
            foreach (var item in settings.PriorityExtensionFiles.Where(e => e.Index > itemToRemove.Index))
            {
                item.Index -= 1;
            }

            // Sauvegarder les param�tres apr�s modification
            return SaveSettings(settings, "itemRemoved", "errorRemovingItem");
        }

        return false;
    }


}

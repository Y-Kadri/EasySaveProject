using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
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

    public static bool SettingChangeLanguage(LanguageEnum language)
    {
        return UpdateSetting(settings => settings.language = language, "languageUpdated", "errorUpdatingLanguage");
    }

    public static bool SettingChangeFormat(EasySave_Library_Log.manager.LogFormatManager.LogFormat format)
    {
        return UpdateSetting(settings => settings.logFormat = format, "formatUpdated", "errorFormatLanguage");
    }

    public static bool SettingChangeMaxLargeFileSize(int value)
    {
        return UpdateSetting(settings => settings.MaxLargeFileSize = value, "formatUpdated", "errorFormatLanguage");
    }

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

    public static string checkFormat(string value, List<string> list)
    {
        if (!string.IsNullOrWhiteSpace(value) && !value.StartsWith('.'))
        {
            value = "." + value;
        }
        return value;
    }

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

    public static List<string> GetList(string key)
    {
        var settings = GetSetting();
        return key switch
        {
            "EncryptedFileExtensions" => settings?.EncryptedFileExtensions ?? new List<string>(),
            "PriorityBusinessProcess" => settings?.PriorityBusinessProcess ?? new List<string>(),
            _ => new List<string>(),
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

        // Trouve l'élément à l'index donné
        var item = settings.PriorityExtensionFiles.FirstOrDefault(x => x.Index == index);
        if (item == null) return;  // Si l'élément n'est pas trouvé, on sort

        // Trouve l'élément précédent dans la liste qui a un index plus petit
        var previousItem = settings.PriorityExtensionFiles
            .Where(x => x.Index < index)
            .OrderByDescending(x => x.Index)  // Trie par index décroissant
            .FirstOrDefault();

        if (previousItem != null)
        {
            // Échange les indices des deux objets
            int tempIndex = item.Index;
            item.Index = previousItem.Index;
            previousItem.Index = tempIndex;

            // Sauvegarde les paramètres
            SaveSettings(settings, "priorityExtensionMovedUp", "errorUpdatingPriorityExtension");
        }
    }



    public static void MovePriorityExtensionFileDown(int index)
    {
        var settings = GetSetting();
        if (settings == null || settings.PriorityExtensionFiles == null || index >= settings.PriorityExtensionFiles.Count) return;

        // Trouve l'élément à l'index donné
        var item = settings.PriorityExtensionFiles.FirstOrDefault(x => x.Index == index);
        if (item == null) return;  // Si l'élément n'est pas trouvé, on sort

        // Trouve l'élément suivant dans la liste qui a un index plus grand
        var nextItem = settings.PriorityExtensionFiles
            .Where(x => x.Index > index)
            .OrderBy(x => x.Index)  // Trie par index croissant
            .FirstOrDefault();

        if (nextItem != null)
        {
            // Échange les indices des deux objets
            int tempIndex = item.Index;
            item.Index = nextItem.Index;
            nextItem.Index = tempIndex;

            // Sauvegarde les paramètres
            SaveSettings(settings, "priorityExtensionMovedDown", "errorUpdatingPriorityExtension");
        }
    }

}

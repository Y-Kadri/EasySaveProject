using System;
using System.IO;
using System.Text.Json;
using EasySave_Library_Log.manager;
using EasySave_Project.Dto;
using EasySave_Project.Model;
using EasySave_Project.Service;

namespace EasySave_Project.Util;

public static class SettingUtil
{
    private static readonly string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "easySave", "easySaveSetting");
    private static readonly string filePath = Path.Combine(directoryPath, "appSetting.json");

    public static bool SettingChangeLanguage(LanguageEnum language)
    {
        return UpdateSetting(settings => settings.language = language, "languageUpdated", "errorUpdatingLanguage");
    }

    public static bool SettingChangeFormat(EasySave_Library_Log.manager.LogFormatManager.LogFormat format)
    {
        return UpdateSetting(settings => settings.logFormat = format, "formatUpdated", "errorFormatLanguage");
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
}


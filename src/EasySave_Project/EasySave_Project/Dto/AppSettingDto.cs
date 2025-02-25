using System.Collections.Generic;
using System.Text.Json.Serialization;
using EasySave_Library_Log.manager;
using EasySave_Project.Model;
using EasySave_Project.Util;

namespace EasySave_Project.Dto;

public class AppSettingDto
{
    [JsonConverter(typeof(EnumConverterUtil.JsonEnumConverter<LanguageEnum>))]
    public LanguageEnum language { get; set; }
    
    [JsonConverter(typeof(EnumConverterUtil.JsonEnumConverter<LogFormatManager.LogFormat>))]
    public LogFormatManager.LogFormat logFormat { get; set; }
    public List<string> EncryptedFileExtensions { get; set; }
    public List<string> PriorityBusinessProcess { get; set; }
    public List<PriorityExtensionDTO> PriorityExtensionFiles { get; set; }
    public int MaxLargeFileSize { get; set; }
    public AppSettingDto(LanguageEnum language, LogFormatManager.LogFormat logFormat)
    {
        this.language = language;
        this.logFormat = logFormat;
        EncryptedFileExtensions = new List<string>();
        PriorityBusinessProcess = new List<string>();
    }
}

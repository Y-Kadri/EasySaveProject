using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave_Project.Model
{
    public enum LanguageEnum
    {
        FR,
        EN
    }

    public static class LanguageEnumExtensions
    {
        public static string GetLanguagePath(this LanguageEnum language)
        {
            switch (language)
            {
                case LanguageEnum.FR:
                    return "../../../Resource/fr.json";
                case LanguageEnum.EN:
                    return "../../../Resource/en.json";
                default:
                    throw new ArgumentOutOfRangeException(nameof(language), language, null);
            }
        }
    }
}

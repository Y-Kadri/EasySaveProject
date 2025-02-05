using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave_Project.Util
{
    /// <summary>
    /// The DateUtil class provides utility methods for working with dates.
    /// It includes predefined date format strings and methods to retrieve the current date formatted according to a specified pattern.
    /// </summary>
    public static class DateUtil
    {
        /// <summary>
        /// A predefined date format string for "yyyy_MM_dd_HH_mm_ss".
        /// </summary>
        public static string YYYY_MM_DD_HH_MM_SS = "yyyy_MM_dd_HH_mm_ss";

        /// <summary>
        /// Gets the current date formatted according to the specified regex pattern.
        /// </summary>
        /// <param name="regex">The format string to apply to the current date.</param>
        /// <returns>The current date as a string formatted according to the specified pattern.</returns>
        public static string GetTodayDate(string regex) => DateTime.Now.ToString(regex);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave_Library_Log.manager
{
    /// <summary>
    /// Singleton class responsible for managing the log format (JSON or XML).
    /// </summary>
    public sealed class LogFormatManager
    {
        private static readonly Lazy<LogFormatManager> instance = new(() => new LogFormatManager());

        /// <summary>
        /// Gets the single instance of LogFormatManager.
        /// </summary>
        public static LogFormatManager Instance => instance.Value;

        /// <summary>
        /// Enumeration to specify log formats.
        /// </summary>
        public enum LogFormat
        {
            JSON,
            XML
        }

        /// <summary>
        /// The current format of the log entries (JSON or XML).
        /// </summary>
        public LogFormat Format { get; private set; } = LogFormat.XML;

        /// <summary>
        /// Private constructor to prevent instantiation from outside.
        /// </summary>
        private LogFormatManager() { }

        /// <summary>
        /// Sets the log format to the specified value.
        /// </summary>
        /// <param name="format">The log format to set.</param>
        public void SetLogFormat(LogFormat format)
        {
            Format = format;
        }
    }
}

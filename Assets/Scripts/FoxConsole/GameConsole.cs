using System;
using System.Collections.Generic;
using UnityEngine;

namespace FoxThorne
{
    public static class GameConsole
    {
        #region settings
        /// <summary>
        /// Should the script log all messages to the unity console as well as the game console?
        /// </summary>
        public static bool unityMirrorLog = true;

        /// <summary>
        /// How many lines will be saved in the console before the old ones get deleted. Negative values mean no lines will be deleted.
        /// </summary>
        public static int maxSavedLines = -1;

        /// <summary>
        /// The string that will be appended onto a string to indicate that it is a verbose message.
        /// </summary>
        public static string VerboseTag { get; } = "<verbose>";
        /// <summary>
        /// The string that will be appended onto a string to indicate that it is a warning message.
        /// </summary>
        public static string WarningTag { get; } = "<warning>";
        /// <summary>
        /// The string that will be appended onto a string to indicate that it is an error message.
        /// </summary>
        public static string ErrorTag { get; } = "<error>";
        #endregion

        public static List<string> lines = new List<string>();

		#region logging methods
		public static void Log(string info, UnityEngine.Object context = null, bool verbose = false)
        {
            if (info.Trim() == "") return;

            if (unityMirrorLog && !verbose)
                Debug.Log(info, context);

            string line = $"<i>({DateTime.Now})</i>: {info}";

            if (verbose)
                line = TagVerbose(line);

            Submit(line);
        }

        public static void LogVerbose(string info, UnityEngine.Object context = null)
        {
            Log(info, context, true);
        }

        public static void LogWarning(string info, UnityEngine.Object context = null, bool verbose = false)
        {
            if (info.Trim() == "") return;

            if (unityMirrorLog && !verbose)
                Debug.LogWarning(info, context);

            string line = $"<i>({DateTime.Now})</i> <color=orange><b>WARNING:</b> {info}</color>";

            if (verbose)
                line = TagVerbose(line);

            line = TagWarning(line);

            Submit(line);
        }

        public static void LogWarningVerbose(string info, UnityEngine.Object context = null)
        {
            LogWarning(info, context, true);
        }

        public static void LogError(string info, UnityEngine.Object context = null, bool verbose = false)
        {
            if (info.Trim() == "") return;

            if (unityMirrorLog && !verbose)
                Debug.LogError(info, context);

            string line = $"<i>({DateTime.Now})</i> <color=red><b>ERROR:</b> {info}</color>";

            if (verbose)
                line = TagVerbose(line);

            line = TagError(line);

            Submit(line);
        }

        public static void LogErrorVerbose(string info, UnityEngine.Object context = null)
        {
            LogError(info, context, true);
        }
		#endregion logging methods

		#region tagging
		static string TagVerbose(string line)
        {
            return line += VerboseTag;
        }

        static string TagWarning(string line)
        {
            return line += WarningTag;
        }

        static string TagError(string line)
        {
            return line += ErrorTag;
        }

		#endregion tagging

		static void Submit(string line)
        {
            lines.Add(line);

            PurgeOldLines();
        }

        static void PurgeOldLines()
        {
            if (maxSavedLines < 0) return;

            while (lines.Count > maxSavedLines)
            {
                lines.RemoveAt(0);
            }
        }
    }
}

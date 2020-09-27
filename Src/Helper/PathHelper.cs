using StartGuildwars2.Global;
using System;
using System.IO;

namespace StartGuildwars2.Helper
{
    public class PathHelper
    {
        public static string SystemDrive = Environment.SystemDirectory.Substring(0, 3);

        public static string GetGameAppDataPathByGamePath(string gamePath)
        {
            return Path.Combine(Path.GetDirectoryName(gamePath), GVar.Instance.ConfigManager.GameAppDataFolderName);
        }

        public static string GetGameAppScreensPathByGamePath(string gamePath)
        {
            return Path.Combine(Path.GetDirectoryName(gamePath), GVar.Instance.ConfigManager.GameAppScreensFolderName);
        }

        public static string GetOriginalAppDataPathByUsername(string username)
        {
            return Path.Combine(SystemDrive, "Users", username, "AppData", "Roaming", GVar.Instance.ConfigManager.MFAppDataFolderName);
        }

        public static string GetOriginalAppScreensPathByUsername(string username)
        {
            return Path.Combine(SystemDrive, "Users", username, "Documents", GVar.Instance.ConfigManager.MFAppDataFolderName, "Screens");
        }
    }
}
using GalaSoft.MvvmLight;
using System;
using System.IO;

namespace StartGuildwars2.Global
{
    public class PathManager : ObservableObject
    {
        public static string AppName = "StartGuildwars2";
        public static string AppConfigFileName = "config.json";

        public PathManager()
        {
            SystemRoamingFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            SystemDocumentFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            SystemTempFolder = Path.GetTempPath();

            AppRoamingFolder = Path.Combine(SystemRoamingFolder, AppName);
            AppTempFolder = Path.Combine(SystemTempFolder, AppName);
            AppCurrentFolder = Environment.CurrentDirectory;
            AppAddonPackageFolder = Path.Combine(AppTempFolder, "Addon Packages");
            AppSetupPackageFolder = Path.Combine(AppTempFolder, "Setup Packages");
            AppConfigFilePath = Path.Combine(AppRoamingFolder, AppConfigFileName);

            ToolHandle64FilePath = Path.Combine(AppCurrentFolder, "Asset", "Tool", "handle64.exe");

            Directory.CreateDirectory(AppRoamingFolder);
            Directory.CreateDirectory(AppTempFolder);
            Directory.CreateDirectory(AppAddonPackageFolder);
            Directory.CreateDirectory(AppSetupPackageFolder);
        }

        // C:\Users\China\AppData\Roaming
        public string SystemRoamingFolder { get; private set; }

        // C:\Users\China\Documents
        public string SystemDocumentFolder { get; private set; }

        // C:\Users\China\AppData\Local\Temp
        public string SystemTempFolder { get; private set; }

        // C:\Users\China\AppData\Roaming\StartGuildwars2
        public string AppRoamingFolder { get; private set; }

        // C:\Users\China\AppData\Local\Temp\StartGuildwars2
        public string AppTempFolder { get; private set; }

        // C:\Program Files\StartGuildwars2
        public string AppCurrentFolder { get; private set; }

        // C:\Users\China\AppData\Roaming\StartGuildwars2\Setup Packages
        public string AppSetupPackageFolder { get; private set; }

        // C:\Users\China\AppData\Roaming\StartGuildwars2\Addon Packages
        public string AppAddonPackageFolder { get; private set; }

        // C:\Users\China\AppData\Roaming\StartGuildwars2\config.json
        public string AppConfigFilePath { get; private set; }

        // C"\Program Files\StartGuildwars2\Asset\Tool\handle64.exe
        public string ToolHandle64FilePath { get; private set; }
    }
}
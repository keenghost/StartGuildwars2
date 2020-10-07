using GalaSoft.MvvmLight;
using Newtonsoft.Json.Linq;
using StartGuildwars2.Helper;
using StartGuildwars2.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace StartGuildwars2.Global
{
    public class ConfigManager : ObservableObject
    {
        public readonly int Version = 1;

        public string GameAppDataFolderName { get; private set; } = "__DATA";
        public string GameAppScreensFolderName { get; private set; } = "__SCREENS";
        public string MFAppDataFolderName { get; private set; } = "Guild Wars 2";
        public string GFAppDataFolderName { get; private set; } = "Guild Wars 2";
        public string MFAppScreensFolderName { get; private set; } = "Guild Wars 2";
        public string GFAppScreensFolderName { get; private set; } = "Guild Wars 2";

        public string MFPath { get; private set; }
        public string GFPath { get; private set; }
        public ObservableCollection<StartupArgumentModel> MFStartupArgumentList { get; private set; } = new ObservableCollection<StartupArgumentModel>();
        public ObservableCollection<StartupArgumentModel> GFStartupArgumentList { get; private set; } = new ObservableCollection<StartupArgumentModel>();
        public ObservableCollection<InstalledAddonItemModel> MFInstalledAddonList { get; private set; } = new ObservableCollection<InstalledAddonItemModel>();
        public ObservableCollection<InstalledAddonItemModel> GFInstalledAddonList { get; private set; } = new ObservableCollection<InstalledAddonItemModel>();
        public string MFUsername { get; private set; }
        public string MFPassword { get; private set; }
        public bool ExitOnStartup { get; private set; }
        public bool CheckUpdateOnStartup { get; private set; }

        public bool IsRunningMF { get; private set; } = false;
        public bool IsRunningGF { get; private set; } = false;

        public ConfigManager()
        {
            configFilePath = GVar.Instance.PathManager.AppConfigFilePath;

            InitConfig();
        }

        private readonly string configFilePath;

        private void InitConfig()
        {
            var savedConfig = UtilHelper.ReadJsonFile(configFilePath);

            MFPath = (savedConfig["MFPath"] ?? "").ToString();
            GFPath = (savedConfig["GFPath"] ?? "").ToString();
            MFUsername = (savedConfig["MFUsername"] ?? "").ToString();
            MFPassword = (savedConfig["MFPassword"] ?? "").ToString();
            ExitOnStartup = (bool)(savedConfig["ExitOnStartup"] ?? true);
            CheckUpdateOnStartup = (bool)(savedConfig["CheckUpdateOnStartup"] ?? true);

            var rawMFStartupArgumentList = savedConfig["MFStartupArgumentList"] == null ? new List<JToken>() : savedConfig["MFStartupArgumentList"].ToList();
            rawMFStartupArgumentList.ForEach(item =>
            {
                MFStartupArgumentList.Add(item.ToObject<StartupArgumentModel>());
            });

            var rawGFStartupArgumentList = savedConfig["GFStartupArgumentList"] == null ? new List<JToken>() : savedConfig["GFStartupArgumentList"].ToList();
            rawGFStartupArgumentList.ForEach(item =>
            {
                GFStartupArgumentList.Add(item.ToObject<StartupArgumentModel>());
            });

            var rawMFInstalledAddonList = savedConfig["MFInstalledAddonList"] == null ? new List<JToken>() : savedConfig["MFInstalledAddonList"].ToList();
            rawMFInstalledAddonList.ForEach(item =>
            {
                MFInstalledAddonList.Add(item.ToObject<InstalledAddonItemModel>());
            });

            var rawGFInstalledAddonList = savedConfig["GFInstalledAddonList"] == null ? new List<JToken>() : savedConfig["GFInstalledAddonList"].ToList();
            rawGFInstalledAddonList.ForEach(item =>
            {
                GFInstalledAddonList.Add(item.ToObject<InstalledAddonItemModel>());
            });
        }

        private bool SaveConfig()
        {
            JObject jObject = new JObject();

            jObject["Version"] = Version;
            jObject["MFPath"] = MFPath;
            jObject["GFPath"] = GFPath;
            jObject["MFUsername"] = MFUsername;
            jObject["MFPassword"] = MFPassword;
            jObject["ExitOnStartup"] = ExitOnStartup;
            jObject["CheckUpdateOnStartup"] = CheckUpdateOnStartup;
            jObject["MFStartupArgumentList"] = JToken.FromObject(MFStartupArgumentList);
            jObject["GFStartupArgumentList"] = JToken.FromObject(GFStartupArgumentList);
            jObject["MFInstalledAddonList"] = JToken.FromObject(MFInstalledAddonList);
            jObject["GFInstalledAddonList"] = JToken.FromObject(GFInstalledAddonList);

            return UtilHelper.WriteJsonFile(configFilePath, jObject);
        }

        public bool SaveStartupArgumentList(List<StartupArgumentModel> list, string type)
        {
            switch (type)
            {
                case "MF":
                    MFStartupArgumentList = new ObservableCollection<StartupArgumentModel>(list);
                    break;

                case "GF":
                    GFStartupArgumentList = new ObservableCollection<StartupArgumentModel>(list);
                    break;

                default:
                    break;
            }

            return SaveConfig();
        }

        public bool SaveInstalledAddonList(List<InstalledAddonItemModel> list, string type)
        {
            switch (type)
            {
                case "MF":
                    MFInstalledAddonList = new ObservableCollection<InstalledAddonItemModel>(list);
                    break;

                case "GF":
                    GFInstalledAddonList = new ObservableCollection<InstalledAddonItemModel>(list);
                    break;

                default:
                    break;
            }

            return SaveConfig();
        }

        public void UpdateRunningState(bool isRunning, string type)
        {
            switch (type)
            {
                case "MF":
                    IsRunningMF = isRunning;
                    break;

                case "GF":
                    IsRunningGF = isRunning;
                    break;

                default:
                    break;
            }
        }

        public bool SaveGamePath(string path, string type)
        {
            switch (type)
            {
                case "MF":
                    MFPath = path;
                    break;

                case "GF":
                    GFPath = path;
                    break;

                default:
                    break;
            }

            return SaveConfig();
        }

        public bool SaveMFUser(string username, string password)
        {
            MFUsername = username;
            MFPassword = password;

            return SaveConfig();
        }

        public bool SaveExitOnStartup(bool value)
        {
            ExitOnStartup = value;

            return SaveConfig();
        }

        public bool SaveCheckUpdateOnStartup(bool value)
        {
            CheckUpdateOnStartup = value;

            return SaveConfig();
        }

        public bool ResetLauncher(string type)
        {
            switch (type)
            {
                case "MF":
                    MFPath = "";
                    MFUsername = "";
                    MFPassword = "";
                    MFStartupArgumentList.Clear();
                    MFInstalledAddonList.Clear();
                    break;

                case "GF":
                    GFPath = "";
                    GFStartupArgumentList.Clear();
                    GFInstalledAddonList.Clear();
                    break;
            }

            return SaveConfig();
        }
    }
}
using GalaSoft.MvvmLight;
using Newtonsoft.Json.Linq;
using StartGuildwars2.Helper;
using StartGuildwars2.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;

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
        public string MFUsername { get; private set; }
        public string MFPassword { get; private set; }
        public bool ExitOnStartup { get; private set; }
        public bool CheckUpdateOnStartup { get; private set; }
        public bool CheckAddonUpdateOnStartup { get; private set; }

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
            CheckAddonUpdateOnStartup = (bool)(savedConfig["CheckAddonUpdateOnStartup"] ?? true);

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
            jObject["CheckAddonUpdateOnStartup"] = CheckAddonUpdateOnStartup;
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

        public bool SaveCheckAddonUpdateOnStartup(bool Value)
        {
            CheckAddonUpdateOnStartup = Value;

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

        // Addon Part
        public bool MFAddonListLoading { get; private set; } = false;
        public bool GFAddonListLoading { get; private set; } = false;
        public List<AddonItemModel> MFAddonList { get; private set; } = new List<AddonItemModel>();
        public List<AddonItemModel> GFAddonList { get; private set; } = new List<AddonItemModel>();
        public ObservableCollection<DisplayAddonItemModel> MFDisplayAddonList { get; private set; } = new ObservableCollection<DisplayAddonItemModel>();
        public ObservableCollection<DisplayAddonItemModel> GFDisplayAddonList { get; private set; } = new ObservableCollection<DisplayAddonItemModel>();
        public ObservableCollection<InstalledAddonItemModel> MFInstalledAddonList { get; private set; } = new ObservableCollection<InstalledAddonItemModel>();
        public ObservableCollection<InstalledAddonItemModel> GFInstalledAddonList { get; private set; } = new ObservableCollection<InstalledAddonItemModel>();
        public bool MFAddonHasUpdate { get; private set; } = false;
        public bool GFAddonHasUpdate { get; private set; } = false;
        public long LastMFAddonListSuccessTime { get; private set; } = 0;
        public long LastGFAddonListSuccessTime { get; private set; } = 0;

        // 对比配置中已安装插件和激战2目录中已安装插件是否匹配
        // 应当只在程序启动时执行一次
        public void CheckInstalledAddonList(string GameType)
        {
            var InstalledAddons = GetInstalledAddonList(GameType);
            var GamePath = GameType == "MF" ? MFPath : GFPath;
            var ShouldDeleteNames = new List<string>();

            foreach (var InstalledAddon in InstalledAddons)
            {
                var MainDllPath = Path.Combine(GamePath, InstalledAddon.MainDll);

                if (!File.Exists(MainDllPath))
                {
                    ShouldDeleteNames.Add(InstalledAddon.Name);
                    return;
                }

                InstalledAddon.Version = UtilHelper.GetExeFileVersion(MainDllPath);
            }

            foreach (var ShouldDeleteName in ShouldDeleteNames)
            {
                InstalledAddons = InstalledAddons.Where(Item => Item.Name != ShouldDeleteName).ToList();
            }

            SaveInstalledAddonList(InstalledAddons, GameType);
        }

        public void FetchAddonList(string GameType)
        {
            if (GameType == "MF" && MFAddonListLoading)
            {
                return;
            }
            else if (GameType == "GF" && GFAddonListLoading)
            {
                return;
            }

            if (GameType == "MF")
            {
                if (UtilHelper.GetTimestampNow() - LastMFAddonListSuccessTime < 60000)
                {
                    return;
                }
            }
            else
            {
                if (UtilHelper.GetTimestampNow() - LastGFAddonListSuccessTime < 60000)
                {
                    return;
                }
            }

            if (GameType == "MF")
            {
                MFAddonListLoading = true;
            }
            else
            {
                GFAddonListLoading = true;
            }

            HttpHelper.GetAsync(new RequestGetModel<List<AddonItemModel>>
            {
                Path = "/api/v1/sgw2/addons",
                Query = new Dictionary<string, string>{
                    { "type", GameType },
                },
                SuccessCallback = Res =>
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        if (GameType == "MF")
                        {
                            MFAddonList.Clear();
                            foreach (var TempAddon in Res.result)
                            {
                                MFAddonList.Add(TempAddon);
                            }
                            LastMFAddonListSuccessTime = UtilHelper.GetTimestampNow();
                        }
                        else
                        {
                            GFAddonList.Clear();
                            foreach (var TempAddon in Res.result)
                            {
                                GFAddonList.Add(TempAddon);
                            }
                            LastGFAddonListSuccessTime = UtilHelper.GetTimestampNow();
                        }

                        UpdateDisplayAddonList(GameType);
                    });
                },
                CompleteCallback = () =>
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        if (GameType == "MF")
                        {
                            MFAddonListLoading = false;
                        }
                        else
                        {
                            GFAddonListLoading = false;
                        }
                    });
                },
            });
        }

        public void UpdateDisplayAddonList(string GameType)
        {
            var InstalledAddons = GetInstalledAddonList(GameType);
            var DisplayAddons = new ObservableCollection<DisplayAddonItemModel>();
            var TempAddonList = GameType == "MF" ? MFAddonList : GFAddonList;
            var HasUpdateFlag = false;

            foreach (var Addon in TempAddonList)
            {
                var DisplayAddon = new DisplayAddonItemModel
                {
                    Name = Addon.name,
                    DisplayName = Addon.displayName,
                    Description = Addon.description,
                    Version = Addon.version,
                    Website = Addon.website,
                    IsZh = Addon.zh,
                    IsInstalled = false,
                    CanInstall = true,
                    CanUpdate = false,
                };
                var InstalledAddon = InstalledAddons.Find(Item => Item.Name == Addon.name);

                if (InstalledAddon != null)
                {
                    DisplayAddon.IsInstalled = true;
                }

                if (Addon.outdated)
                {
                    DisplayAddons.Add(DisplayAddon);
                    continue;
                }

                if (DisplayAddon.IsInstalled)
                {
                    if (UtilHelper.GetVersionWeight(Addon.version) > UtilHelper.GetVersionWeight(InstalledAddon.Version))
                    {
                        DisplayAddon.CanUpdate = true;
                        HasUpdateFlag = true;
                    }

                    DisplayAddons.Add(DisplayAddon);
                    continue;
                }

                var CanInstall = true;
                var ConflictDisplayNames = new List<string>();

                foreach (var ConflictAddonName in Addon.conflict)
                {
                    if (InstalledAddons.Find(item => item.Name == ConflictAddonName) != null)
                    {
                        CanInstall = false;
                        var ConfligAddon = TempAddonList.Find(Item => Item.name == ConflictAddonName);
                        ConflictDisplayNames.Add(ConflictAddonName);
                    }
                }

                DisplayAddon.CanInstall = CanInstall;
                if (ConflictDisplayNames.Count > 0)
                {
                    var ConflictDescription = "与已安装插件";

                    foreach (var ConflictDisplayName in ConflictDisplayNames)
                    {
                        ConflictDescription += "“" + ConflictDisplayName + "”";
                    }

                    ConflictDescription += "冲突";
                    DisplayAddon.ConflictDescription = ConflictDescription;
                }

                DisplayAddons.Add(DisplayAddon);
            }

            if (GameType == "MF")
            {
                MFDisplayAddonList.Clear();
                foreach (var TempDisplayAddon in DisplayAddons)
                {
                    MFDisplayAddonList.Add(TempDisplayAddon);
                }
                MFAddonHasUpdate = HasUpdateFlag;
            }
            else
            {
                GFDisplayAddonList.Clear();
                foreach (var TempDisplayAddon in DisplayAddons)
                {
                    GFDisplayAddonList.Add(TempDisplayAddon);
                }
                GFAddonHasUpdate = HasUpdateFlag;
            }
        }

        public List<AddonItemModel> GetAddonList(string GameType)
        {
            switch (GameType)
            {
                case "MF":
                    return new List<AddonItemModel>(MFAddonList);

                case "GF":
                    return new List<AddonItemModel>(GFAddonList);

                default:
                    return new List<AddonItemModel>();
            }
        }

        public ObservableCollection<DisplayAddonItemModel> GetDisplayAddonList(string GameType)
        {
            switch (GameType)
            {
                case "MF":
                    return MFDisplayAddonList;

                case "GF":
                    return GFDisplayAddonList;

                default:
                    return new ObservableCollection<DisplayAddonItemModel>();
            }
        }

        public List<InstalledAddonItemModel> GetInstalledAddonList(string GameType)
        {
            switch (GameType)
            {
                case "MF":
                    return new List<InstalledAddonItemModel>(MFInstalledAddonList);

                case "GF":
                    return new List<InstalledAddonItemModel>(GFInstalledAddonList);

                default:
                    return new List<InstalledAddonItemModel>();
            }
        }

        public bool SaveInstalledAddonList(List<InstalledAddonItemModel> List, string Type)
        {
            switch (Type)
            {
                case "MF":
                    MFInstalledAddonList = new ObservableCollection<InstalledAddonItemModel>(List);
                    break;

                case "GF":
                    GFInstalledAddonList = new ObservableCollection<InstalledAddonItemModel>(List);
                    break;

                default:
                    break;
            }

            return SaveConfig();
        }
    }
}
using GalaSoft.MvvmLight.Command;
using HandyControl.Controls;
using HandyControl.Tools.Extension;
using StartGuildwars2.Global;
using StartGuildwars2.Helper;
using StartGuildwars2.Model;
using StartGuildwars2.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace StartGuildwars2.ViewModel
{
    public class ComAddonsDialogViewModel : BaseDialogDataViewModel
    {
        private string GameType { get; set; }
        private readonly ConfigManager _ConfigManager;

        public string DialogTitle { get; private set; }
        public List<AddonItemModel> Addons { get; private set; }
        public ObservableCollection<DisplayAddonItemModel> DisplayAddons { get; private set; }
        public bool Loading { get; private set; } = true;

        public RelayCommand<string> HyperlinkCommand => new Lazy<RelayCommand<string>>(() => new RelayCommand<string>(Hyperlink)).Value;
        public RelayCommand<string> InstallCommand => new Lazy<RelayCommand<string>>(() => new RelayCommand<string>(Install)).Value;
        public RelayCommand<string> UpdateCommand => new Lazy<RelayCommand<string>>(() => new RelayCommand<string>(Update)).Value;
        public RelayCommand<string> UninstallCommand => new Lazy<RelayCommand<string>>(() => new RelayCommand<string>(Uninstall)).Value;

        public ComAddonsDialogViewModel()
        {
            _ConfigManager = GVar.Instance.ConfigManager;
        }

        public void Prepare(string type)
        {
            GameType = type;
            DialogTitle = "插件管理（" + (GameType == "GF" ? "国服" : "美服") + "）";

            Thread prepareListThread = new Thread(() =>
            {
                CheckInstalledAddons();

                Thread.Sleep(UtilHelper.GetRandomNumber(2000, 4000));

                GetList();
            });

            prepareListThread.Start();
        }

        private void CheckInstalledAddons()
        {
            var installedAddons = GetInstalledAddons();
            var gamePath = GameType == "MF" ? _ConfigManager.MFPath : _ConfigManager.GFPath;
            var shouldDeleteNames = new List<string>();

            foreach (var installedAddon in installedAddons)
            {
                var mainDllPath = Path.Combine(gamePath, installedAddon.MainDll);

                if (!File.Exists(mainDllPath))
                {
                    shouldDeleteNames.Add(installedAddon.Name);
                    return;
                }

                installedAddon.Version = UtilHelper.GetExeFileVersion(mainDllPath);
            }

            foreach (var shouldDeleteName in shouldDeleteNames)
            {
                installedAddons = installedAddons.Where(item => item.Name != shouldDeleteName).ToList();
            }

            _ConfigManager.SaveInstalledAddonList(installedAddons, GameType);
        }

        private void GetList()
        {
            HttpHelper.GetAsync(new RequestGetModel<List<AddonItemModel>>
            {
                Path = "/api/v1/sgw2/addons",
                Query = new Dictionary<string, string>{
                    { "type", GameType },
                },
                SuccessCallback = res =>
                {
                    Addons = new List<AddonItemModel>(res.result);
                    UpdateDisplayAddonList();

                    Loading = false;
                },
            });
        }

        private void UpdateDisplayAddonList()
        {
            var installedAddons = GetInstalledAddons();
            var displayAddons = new ObservableCollection<DisplayAddonItemModel>();

            foreach (var addon in Addons)
            {
                var displayAddon = new DisplayAddonItemModel
                {
                    Name = addon.name,
                    DisplayName = addon.displayName,
                    Description = addon.description,
                    Version = addon.version,
                    Website = addon.website,
                    IsZh = addon.zh,
                    IsInstalled = false,
                    CanInstall = true,
                    CanUpdate = false,
                };
                var installedAddon = installedAddons.Find(item => item.Name == addon.name);

                if (installedAddon != null)
                {
                    displayAddon.IsInstalled = true;
                }

                if (addon.outdated)
                {
                    displayAddons.Add(displayAddon);
                    continue;
                }

                if (displayAddon.IsInstalled)
                {
                    if (UtilHelper.GetVersionWeight(addon.version) > UtilHelper.GetVersionWeight(installedAddon.Version))
                    {
                        displayAddon.CanUpdate = true;
                    }

                    displayAddons.Add(displayAddon);
                    continue;
                }

                var canInstall = true;
                var conflictDisplayNames = new List<string>();

                foreach (var conflictAddonName in addon.conflict)
                {
                    if (installedAddons.Find(item => item.Name == conflictAddonName) != null)
                    {
                        canInstall = false;
                        var confligAddon = Addons.Find(item => item.name == conflictAddonName);
                        conflictDisplayNames.Add(conflictAddonName);
                    }
                }

                displayAddon.CanInstall = canInstall;
                if (conflictDisplayNames.Count > 0)
                {
                    var conflictDescription = "与已安装插件";

                    foreach (var conflictDisplayName in conflictDisplayNames)
                    {
                        conflictDescription += "“" + conflictDisplayName + "”";
                    }

                    conflictDescription += "冲突";
                    displayAddon.ConflictDescription = conflictDescription;
                }

                displayAddons.Add(displayAddon);
            }

            DisplayAddons = displayAddons;
        }

        private List<InstalledAddonItemModel> GetInstalledAddons()
        {
            switch (GameType)
            {
                case "MF":
                    return new List<InstalledAddonItemModel>(_ConfigManager.MFInstalledAddonList);

                case "GF":
                    return new List<InstalledAddonItemModel>(_ConfigManager.GFInstalledAddonList);

                default:
                    return new List<InstalledAddonItemModel>();
            }
        }

        private void Hyperlink(string uri)
        {
            Process.Start(uri);
        }

        private void Install(string name)
        {
            var willInstallAddon = Addons.Find(item => item.name == name);
            var applicationVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            if (willInstallAddon == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(willInstallAddon.limitExeVersion))
            {
                if (UtilHelper.GetVersionWeight(applicationVersion) < UtilHelper.GetVersionWeight(willInstallAddon.limitExeVersion))
                {
                    UtilHelper.ShowAlertDialog(new AlertDialogInterfaceModel { Content = "StartGuildwars2 版本太低，请先更新" });
                    return;
                }
            }

            var willInstallAddonList = GetWillInstallList(name);
            var steps = new List<AddonStepModel>();

            foreach (var willInstallAddonName in willInstallAddonList)
            {
                steps.Add(new AddonStepModel
                {
                    Action = "INSTALL",
                    AddonName = willInstallAddonName,
                });
            }

            ShowAddonProgressDialog(steps);
        }

        private List<string> GetWillInstallList(string name)
        {
            var list = new List<string>();
            var installedList = GetInstalledAddons();
            var addon = Addons.Find(item => item.name == name);

            foreach (var d in addon.dependency)
            {
                if (installedList.Find(item => item.Name == d) == null)
                {
                    list.AddRange(GetWillInstallList(d));
                }
            }

            return AddDependency(list, name);
        }

        private void Update(string name)
        {
            var applicationVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            var installedAddons = GetInstalledAddons();
            var willInstallAddon = Addons.Find(item => item.name == name);
            var willUninstallAddon = installedAddons.Find(item => item.Name == name);
            var steps = new List<AddonStepModel>();

            if (willInstallAddon == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(willInstallAddon.limitExeVersion))
            {
                if (UtilHelper.GetVersionWeight(applicationVersion) < UtilHelper.GetVersionWeight(willInstallAddon.limitExeVersion))
                {
                    UtilHelper.ShowAlertDialog(new AlertDialogInterfaceModel { Content = "StartGuildwars2 版本太低，请先更新" });
                    return;
                }
            }

            if (UtilHelper.GetVersionWeight(willInstallAddon.updateRequiredLastVersion) > UtilHelper.GetVersionWeight(willUninstallAddon.Version))
            {
                steps.Add(new AddonStepModel
                {
                    Action = "UNINSTALL",
                    AddonName = name,
                });
            }

            var willInstallAddonList = GetWillInstallList(name);

            foreach (var willInstallAddonName in willInstallAddonList)
            {
                steps.Add(new AddonStepModel
                {
                    Action = "INSTALL",
                    AddonName = willInstallAddonName,
                });
            }

            ShowAddonProgressDialog(steps);
        }

        private void Uninstall(string name)
        {
            var willUninstallAddon = Addons.Find(item => item.name == name);

            if (willUninstallAddon == null)
            {
                return;
            }

            var willUninstallAddonList = GetWillUninstallList(name);
            var steps = new List<AddonStepModel>();

            foreach (var willUninstallAddonName in willUninstallAddonList)
            {
                steps.Add(new AddonStepModel
                {
                    Action = "UNINSTALL",
                    AddonName = willUninstallAddonName,
                });
            }

            ShowAddonProgressDialog(steps);
        }

        private List<string> GetWillUninstallList(string name)
        {
            var list = new List<string>();
            var installedList = GetInstalledAddons();

            foreach (var addon in Addons)
            {
                if (addon.dependency.FindIndex(item => item == name) != -1 && installedList.Find(item => item.Name == addon.name) != null)
                {
                    list.AddRange(GetWillUninstallList(addon.name));
                }
            }

            return AddDependency(list, name);
        }

        private List<string> AddDependency(List<string> formerDependencies, string dependency)
        {
            if (formerDependencies.IndexOf(dependency) != -1)
            {
                return formerDependencies;
            }

            formerDependencies.Add(dependency);
            return formerDependencies;
        }

        private void ShowAddonProgressDialog(List<AddonStepModel> steps)
        {
            Dialog.Show<ComAddonsProgressDialogView>("DialogContainer").Initialize<ComAddonsProgressDialogViewModel>(vm =>
            {
                vm.DialogCallback = (shouldUpdate) =>
                {
                    if ((bool)shouldUpdate)
                    {
                        UpdateDisplayAddonList();
                    }
                };

                vm.Prepare(new AddonProgressMessageModel
                {
                    Type = GameType,
                    Addons = new List<AddonItemModel>(Addons),
                    AddonSteps = steps,
                });
            });
        }
    }
}
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
        public ConfigManager _ConfigManager { get; private set; }

        public string DialogTitle { get; private set; }
        public ObservableCollection<DisplayAddonItemModel> DisplayAddons => _ConfigManager.GetDisplayAddonList(GameType);
        public bool IsMF => GameType == "MF";

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

            _ConfigManager.FetchAddonList(GameType);
        }

        private void Hyperlink(string uri)
        {
            Process.Start(uri);
        }

        private void Install(string name)
        {
            var TempAddonList = _ConfigManager.GetAddonList(GameType);
            var willInstallAddon = TempAddonList.Find(item => item.name == name);
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
            var TempAddonList = _ConfigManager.GetAddonList(GameType);
            var installedList = _ConfigManager.GetInstalledAddonList(GameType);
            var addon = TempAddonList.Find(item => item.name == name);

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
            var TempAddonList = _ConfigManager.GetAddonList(GameType);
            var installedAddons = _ConfigManager.GetInstalledAddonList(GameType);
            var willInstallAddon = TempAddonList.Find(item => item.name == name);
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
            var TempAddonList = _ConfigManager.GetAddonList(GameType);
            var willUninstallAddon = TempAddonList.Find(item => item.name == name);

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
            var TempAddonList = _ConfigManager.GetAddonList(GameType);
            var installedList = _ConfigManager.GetInstalledAddonList(GameType);

            foreach (var addon in TempAddonList)
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
            var willUninstallList = steps.Where(item => item.Action == "UNINSTALL").ToList();
            var willInstallList = steps.Where(item => item.Action == "INSTALL").ToList();
            var content = "";

            if (willUninstallList.Count > 0)
            {
                content += "将卸载以下插件:\r\n";

                willUninstallList.ForEach(item =>
                {
                    content = content + "- " + item.AddonName + "\r\n";
                });

                content += "\r\n";
            }

            if (willInstallList.Count > 0)
            {
                content += "将安装以下插件:\r\n";

                willInstallList.ForEach(item =>
                {
                    content = content + "- " + item.AddonName + "\r\n";
                });
            }

            UtilHelper.ShowConfirmDialog(new ConfirmDialogInterfaceModel
            {
                Content = content,
                ConfirmCallback = () =>
                {
                    Dialog.Show<ComAddonsProgressDialogView>("DialogContainer").Initialize<ComAddonsProgressDialogViewModel>(vm =>
                    {
                        vm.DialogCallback = (shouldUpdate) =>
                        {
                            if ((bool)shouldUpdate)
                            {
                                _ConfigManager.UpdateDisplayAddonList(GameType);
                            }
                        };

                        vm.Prepare(new AddonProgressMessageModel
                        {
                            Type = GameType,
                            Addons = _ConfigManager.GetAddonList(GameType),
                            AddonSteps = steps,
                        });
                    });
                },
            });
        }
    }
}
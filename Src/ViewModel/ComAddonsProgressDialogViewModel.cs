using GalaSoft.MvvmLight.Command;
using HandyControl.Interactivity;
using StartGuildwars2.Global;
using StartGuildwars2.Helper;
using StartGuildwars2.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Input;

namespace StartGuildwars2.ViewModel
{
    public class ComAddonsProgressDialogViewModel : BaseDialogDataViewModel
    {
        private readonly ConfigManager _ConfigManager;
        private readonly PathManager _PathManager;

        public string Type { get; set; }
        public List<AddonStepModel> AddonSteps { get; set; }
        public List<AddonItemModel> Addons { get; set; }
        public string ProgressText { get; set; }
        public bool ShowFinishButton { get; set; }
        public bool ShowCloseButton { get; set; }

        public RelayCommand CompleteCommand => new Lazy<RelayCommand>(() => new RelayCommand(Complete)).Value;

        public ComAddonsProgressDialogViewModel()
        {
            _ConfigManager = GVar.Instance.ConfigManager;
            _PathManager = GVar.Instance.PathManager;
        }

        public void Prepare(AddonProgressMessageModel message)
        {
            Type = message.Type;
            Addons = message.Addons;
            AddonSteps = message.AddonSteps;

            Thread StepThread = new Thread(() =>
            {
                DoSteps();
            });

            StepThread.Start();
        }

        private void Complete()
        {
            DialogCallback?.Invoke(true);
            ((ICommand)ControlCommands.Close).Execute(null);
        }

        private void DoSteps()
        {
            DoStep(0, AddonSteps);
        }

        private void DoStep(int stepIndex, List<AddonStepModel> steps)
        {
            if (stepIndex == steps.Count)
            {
                ProgressEnd(true);
                return;
            }

            var step = steps[stepIndex];
            var action = step.Action;
            var addonName = step.AddonName;

            if (action == "UNINSTALL")
            {
                var installedAddons = GetInstalledAddons();
                var installedAddon = installedAddons.Find(item => item.Name == addonName);
                var gameFolderPath = Path.GetDirectoryName(Type == "MF" ? _ConfigManager.MFPath : _ConfigManager.GFPath);

                Output("");
                Output("- 卸载 " + installedAddon.Name);

                foreach (var filepath in installedAddon.Uninstall)
                {
                    var deletePath = Path.Combine(gameFolderPath, Regex.Replace(filepath, @"^\/", ""));

                    Output("   * 移除");
                    Output("      " + deletePath);
                    IOHelper.DeleteFileOrDirectory(deletePath);
                }

                UpdateConfig(action, addonName);

                DoStep(++stepIndex, steps);
            }
            else if (action == "INSTALL")
            {
                var addon = Addons.Find(item => item.name == addonName);
                var addonPackagePath = Path.Combine(_PathManager.AppAddonPackageFolder, addon.name + "-" + addon.version + ".zip");
                var gameFolderPath = Path.GetDirectoryName(Type == "MF" ? _ConfigManager.MFPath : _ConfigManager.GFPath);

                Output("");
                Output("- 安装 " + addon.displayName);

                if (File.Exists(addonPackagePath))
                {
                    Output("   * 检测到安装包缓存");
                    Output("      " + addonPackagePath);
                    Output("   * 开始解压 " + addon.displayName);

                    IOHelper.ExtractZipFile(addonPackagePath, gameFolderPath);

                    Output("   * 安装 " + addon.displayName + " 完成");

                    UpdateConfig(action, addonName);

                    DoStep(++stepIndex, steps);
                }
                else
                {
                    Output("   * 开始下载安装包 " + addon.displayName);

                    HttpHelper.DownloadFileAsync(new RequestDownloadFileModel
                    {
                        RemoteUrl = addon.src,
                        LocalPath = addonPackagePath,
                        SuccessCallback = () =>
                        {
                            Output("   * 下载安装包 " + addon.displayName + " 完成");
                            Output("   * 开始解压 " + addon.displayName);

                            IOHelper.ExtractZipFile(addonPackagePath, gameFolderPath);

                            Output("   * 安装 " + addon.displayName + " 完成");

                            UpdateConfig(action, addonName);

                            DoStep(++stepIndex, steps);
                        },
                        ErrorCallback = err =>
                        {
                            Output("   * 下载安装包 " + addon.displayName + " 失败");
                            ProgressEnd(false);
                        },
                    });
                }
            }
        }

        private void UpdateConfig(string action, string name)
        {
            var list = GetInstalledAddons();

            switch (action)
            {
                case "UNINSTALL":
                    list = list.Where(item => item.Name != name).ToList();

                    break;

                case "INSTALL":
                    var addon = Addons.Find(item => item.name == name);
                    list = list.Where(item => item.Name != name).ToList();
                    list.Add(new InstalledAddonItemModel
                    {
                        Name = addon.name,
                        MainDll = addon.mainDll,
                        Version = addon.version,
                        Uninstall = addon.uninstall,
                    });

                    break;
            }

            _ConfigManager.SaveInstalledAddonList(list, Type);
        }

        private void ProgressEnd(bool allDone)
        {
            if (allDone)
            {
                ShowFinishButton = true;
                Output("");
                Output("- 安装/卸载/更新 已全部完成");
            }
            else
            {
                ShowCloseButton = true;
                Output("");
                Output("- 出现错误，任务已中断");
            }
        }

        private List<InstalledAddonItemModel> GetInstalledAddons()
        {
            switch (Type)
            {
                case "MF":
                    return new List<InstalledAddonItemModel>(_ConfigManager.MFInstalledAddonList);

                case "GF":
                    return new List<InstalledAddonItemModel>(_ConfigManager.GFInstalledAddonList);

                default:
                    return new List<InstalledAddonItemModel>();
            }
        }

        private void Output(string text)
        {
            ProgressText += text + "\r\n";
        }
    }
}
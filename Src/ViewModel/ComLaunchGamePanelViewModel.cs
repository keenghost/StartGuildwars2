using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using HandyControl.Controls;
using HandyControl.Tools.Extension;
using Microsoft.Win32;
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
using System.Threading;

namespace StartGuildwars2.ViewModel
{
    public class ComLaunchGamePanelViewModel : ViewModelBase
    {
        private string _GameType;
        private Process _RunningProcess;
        private readonly ConfigManager _ConfigManager;

        public string TypeText { get; private set; }
        public string GamePath { get; private set; }
        public bool IsMF { get; private set; }
        public bool IsRunning { get; private set; }
        public ObservableCollection<StartupArgumentModel> StartupArguments { get; private set; }

        public RelayCommand InitializeCommand => new Lazy<RelayCommand>(() => new RelayCommand(Initialize)).Value;
        public RelayCommand DeleteCommand => new Lazy<RelayCommand>(() => new RelayCommand(Delete)).Value;
        public RelayCommand<string> EnableStartupArgumentCommand => new Lazy<RelayCommand<string>>(() => new RelayCommand<string>(EnableStartupArgument)).Value;
        public RelayCommand<string> DisableStartupArgumentCommand => new Lazy<RelayCommand<string>>(() => new RelayCommand<string>(DisableStartupArgument)).Value;
        public RelayCommand OpenAddonsDialogCommand => new Lazy<RelayCommand>(() => new RelayCommand(OpenAddonsDialog)).Value;
        public RelayCommand OpenStartupArgumentsDialogCommand => new Lazy<RelayCommand>(() => new RelayCommand(OpenStartupArgumentsDialog)).Value;
        public RelayCommand LaunchGameCommand => new Lazy<RelayCommand>(() => new RelayCommand(LaunchGame)).Value;
        public RelayCommand KillGameCommand => new Lazy<RelayCommand>(() => new RelayCommand(KillGame)).Value;

        public ComLaunchGamePanelViewModel()
        {
            _ConfigManager = GVar.Instance.ConfigManager;
        }

        public string GameType
        {
            get
            {
                return _GameType;
            }
            set
            {
                _GameType = value;
                UpdateDisplay();
            }
        }

        private void UpdateDisplay()
        {
            switch (GameType)
            {
                case "MF":
                    TypeText = "美服";
                    GamePath = _ConfigManager.MFPath;
                    IsMF = true;
                    StartupArguments = new ObservableCollection<StartupArgumentModel>(_ConfigManager.MFStartupArgumentList);
                    break;

                case "GF":
                    TypeText = "国服";
                    GamePath = _ConfigManager.GFPath;
                    IsMF = false;
                    StartupArguments = new ObservableCollection<StartupArgumentModel>(_ConfigManager.GFStartupArgumentList);
                    break;
            }

            CheckGameRunningState();
        }

        private void Initialize()
        {
            if (IsMF)
            {
                Dialog.Show<ComInitializeMFDialogView>("DialogContainer").Initialize<ComInitializeMFDialogViewModel>(vm =>
                {
                    vm.DialogCallback = (shouldUpdate) =>
                    {
                        if ((bool)shouldUpdate)
                        {
                            UpdateDisplay();
                        }
                    };
                });
            }
            else
            {
                var openFileDialog = new OpenFileDialog()
                {
                    Title = "选择激战2主程序",
                    Filter = "激战2主程序|Gw2-64.exe",
                };

                var result = openFileDialog.ShowDialog();

                if (result == true)
                {
                    var filepath = openFileDialog.FileName;
                    var directory = Path.GetDirectoryName(filepath);

                    if (!string.IsNullOrEmpty(_ConfigManager.MFPath))
                    {
                        if (_ConfigManager.MFPath.StartsWith(directory, StringComparison.InvariantCultureIgnoreCase))
                        {
                            UtilHelper.ShowAlertDialog(new AlertDialogInterfaceModel { Content = "美服已选择此路径，请重新选择其它路径" });
                            return;
                        }
                    }

                    _ConfigManager.SaveGamePath(filepath, "GF");

                    UpdateDisplay();
                }
            }
        }

        private void Delete()
        {
            UtilHelper.ShowConfirmDialog(new ConfirmDialogInterfaceModel
            {
                Content = "将删除相关配置，该操作不会影响已安装的插件以及游戏文件。\r\n但已安装插件列表无法识别。\r\n\r\n删除后无法恢复配置，确定删除吗？",
                ConfirmCallback = () =>
                {
                    _ConfigManager.ResetLauncher(GameType);
                    UpdateDisplay();
                },
            });
        }

        private void EnableStartupArgument(string id)
        {
            SaveStartupArgument(id, true);
        }

        private void DisableStartupArgument(string id)
        {
            SaveStartupArgument(id, false);
        }

        private void OpenAddonsDialog()
        {
            Dialog.Show<ComAddonsDialogView>("DialogContainer").Initialize<ComAddonsDialogViewModel>(vm =>
            {
                vm.DialogCallback = (shouldUpdate) =>
                {
                    if ((bool)shouldUpdate)
                    {
                        UpdateDisplay();
                    }
                };

                vm.Prepare(GameType);
            });
        }

        private void OpenStartupArgumentsDialog()
        {
            Dialog.Show<ComStartupArgumentsDialogView>("DialogContainer").Initialize<ComStartupArgumentsDialogViewModel>(vm =>
            {
                vm.DialogCallback = (shouldUpdate) =>
                {
                    if ((bool)shouldUpdate)
                    {
                        UpdateDisplay();
                    }
                };

                vm.Prepare(GameType);
            });
        }

        private void LaunchGame()
        {
            var username = IsMF ? _ConfigManager.MFUsername : Environment.UserName;
            var originalAppScreensPath = PathHelper.GetOriginalAppScreensPathByUsername(username);

            if (GameStateHelper.HasUnknownRunningGame(new List<string> {
                _ConfigManager.MFPath,
                _ConfigManager.GFPath,
            }))
            {
                UtilHelper.ShowAlertDialog(new AlertDialogInterfaceModel { Content = "有未知激战2客户端正在运行" });
                return;
            }

            if (!IOHelper.IsDirectoryLnk(originalAppScreensPath))
            {
                if (!IOHelper.IsDirectoryEmpty(originalAppScreensPath))
                {
                    var content = "创建截图文件夹快捷方式时遇到问题。\r\n\r\n截图文件夹:\r\n";
                    content += originalAppScreensPath;
                    content += "\r\n已有截图，需要对其进行迁移。\r\n\r\n请确认其截图属于美服还是国服，然后从下面选择。";

                    UtilHelper.ShowConfirmDialog(new ConfirmDialogInterfaceModel
                    {
                        Title = "正在启动" + (IsMF ? "美服" : "国服"),
                        Content = content,
                        ConfirmButtonText = "国服",
                        CancelButtonText = "美服",
                        ShowClose = true,
                        ConfirmCallback = () =>
                        {
                            MoveScreensAndLaunch(originalAppScreensPath, _ConfigManager.GFPath);
                        },
                        CancelCallback = () =>
                        {
                            MoveScreensAndLaunch(originalAppScreensPath, _ConfigManager.MFPath);
                        },
                    });

                    return;
                }
            }

            DoLaunchGame();
        }

        private void KillGame()
        {
            try
            {
                //_RunningProcess.Kill();
                //_RunningProcess = null;
                //_ConfigManager.UpdateRunningState(false, GameType);
            }
            catch { }
        }

        private void SaveStartupArgument(string id, bool enable)
        {
            var _list = IsMF ? _ConfigManager.MFStartupArgumentList : _ConfigManager.GFStartupArgumentList;
            var list = new List<StartupArgumentModel>(_list);

            foreach (var item in list)
            {
                if (item.ID == id)
                {
                    item.Enable = enable;
                }
            }

            _ConfigManager.SaveStartupArgumentList(list, GameType);
        }

        private void CheckGameRunningState()
        {
            if (IsRunning)
            {
                return;
            }

            if (!Environment.Is64BitProcess)
            {
                return;
            }

            var Gw2Processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(GamePath));

            foreach (Process p in Gw2Processes)
            {
                var filename = p.MainModule.FileName;
                var filepath = Path.GetDirectoryName(GamePath);

                if (filename.StartsWith(filepath, StringComparison.InvariantCultureIgnoreCase))
                {
                    _RunningProcess = p;
                    _ConfigManager.UpdateRunningState(true, GameType);
                    IsRunning = true;

                    Thread waitForCurrentProcessThread = new Thread(() =>
                    {
                        p.WaitForExit();
                        _RunningProcess = null;
                        _ConfigManager.UpdateRunningState(false, GameType);
                        IsRunning = false;
                    });

                    waitForCurrentProcessThread.Start();
                }
            }
        }

        private void MoveScreensAndLaunch(string source, string targetGamePath)
        {
            if (string.IsNullOrEmpty(targetGamePath))
            {
                UtilHelper.ShowAlertDialog(new AlertDialogInterfaceModel
                {
                    Content = "尚未配置对应游戏路径。",
                });

                return;
            }

            try
            {
                var target = PathHelper.GetGameAppScreensPathByGamePath(targetGamePath);
                IOHelper.MoveFileOrDirectory(source, target);
            }
            catch (Exception e)
            {
                UtilHelper.ShowAlertDialog(new AlertDialogInterfaceModel
                {
                    Content = "移动截图失败:\r\n" + e.Message,
                });

                return;
            }

            DoLaunchGame();
        }

        private void DoLaunchGame()
        {
            var username = IsMF ? _ConfigManager.MFUsername : Environment.UserName;
            var originalAppDataPath = PathHelper.GetOriginalAppDataPathByUsername(username);
            var originalAppScreensPath = PathHelper.GetOriginalAppScreensPathByUsername(username);
            var gameAppDataPath = PathHelper.GetGameAppDataPathByGamePath(GamePath);
            var gameAppScreensPath = PathHelper.GetGameAppScreensPathByGamePath(GamePath);

            if (_ConfigManager.IsRunningMF || _ConfigManager.IsRunningGF)
            {
                if (!GameStateHelper.KillMutant())
                {
                    UtilHelper.ShowAlertDialog(new AlertDialogInterfaceModel { Content = "KillMutant失败" });
                    return;
                }
            }

            Directory.CreateDirectory(gameAppDataPath);
            Directory.CreateDirectory(gameAppScreensPath);

            if (!UtilHelper.RemoveAndCreateDirectoryLink(originalAppDataPath, gameAppDataPath))
            {
                UtilHelper.ShowAlertDialog(new AlertDialogInterfaceModel { Content = "创建配置文件夹链接失败" });
                return;
            }

            if (!UtilHelper.RemoveAndCreateDirectoryLink(originalAppScreensPath, gameAppScreensPath))
            {
                UtilHelper.ShowAlertDialog(new AlertDialogInterfaceModel { Content = "创建截图文件夹链接失败" });
                return;
            }

            if (IsMF)
            {
                Process.Start(new ProcessStartInfo()
                {
                    UserName = _ConfigManager.MFUsername,
                    Password = UtilHelper.ConvertStringToSecureString(_ConfigManager.MFPassword),
                    FileName = _ConfigManager.MFPath,
                    Arguments = string.Join(" ", _ConfigManager.MFStartupArgumentList.Where(item => item.Enable).Select(item => item.Command)),
                    UseShellExecute = false,
                    LoadUserProfile = true,
                });
            }
            else
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = _ConfigManager.GFPath,
                    Arguments = string.Join(" ", _ConfigManager.GFStartupArgumentList.Where(item => item.Enable).Select(item => item.Command)),
                    UseShellExecute = false,
                });
            }

            if (_ConfigManager.ExitOnStartup)
            {
                Environment.Exit(0);
                return;
            }

            CheckGameRunningState();
        }
    }
}
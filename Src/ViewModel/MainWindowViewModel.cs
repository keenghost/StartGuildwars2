using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using StartGuildwars2.Global;
using StartGuildwars2.Helper;
using StartGuildwars2.Model;
using StartGuildwars2.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace StartGuildwars2.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        private readonly ConfigManager _ConfigManager;

        public object CurrentView { get; private set; } = null;

        public RelayCommand CloseCommand => new Lazy<RelayCommand>(() => new RelayCommand(Close)).Value;
        public RelayCommand<string> MenuCommand => new Lazy<RelayCommand<string>>(() => new RelayCommand<string>(SwitchItem)).Value;

        public MainWindowViewModel()
        {
            CheckMutex();

            GVar.Instance.PathManager = new PathManager();
            GVar.Instance.ConfigManager = new ConfigManager();

            _ConfigManager = GVar.Instance.ConfigManager;

            SetCurrentView("LAUNCHER");
            CheckUpdate();
        }

        public List<MenuItemModel> MenuList { get; private set; } = new List<MenuItemModel>(new MenuItemModel[] {
            new MenuItemModel() { Key = "LAUNCHER", Title = "启动器", ViewInstance = null, IsSelected = false, Icon = Application.Current.Resources["AllGeometry"] },
            new MenuItemModel() { Key = "COMMUNITY", Title = "社区", ViewInstance = null, IsSelected = false, Icon = Application.Current.Resources["SearchGeometry"] },
            new MenuItemModel() { Key = "ABOUT", Title = "关于", ViewInstance = null, IsSelected = false, Icon = Application.Current.Resources["InfoGeometry"] },
        });

        private void Close()
        {
            Environment.Exit(0);
        }

        private void SwitchItem(string viewKey)
        {
            SetCurrentView(viewKey);
        }

        private void CheckMutex()
        {
            new Mutex(true, Process.GetCurrentProcess().ProcessName, out bool isRunning);

            if (isRunning)
            {
                var processes = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);

                if (processes.Length > 0)
                {
                    SwitchToThisWindow(processes[0].MainWindowHandle, true);
                }

                Environment.Exit(1);
            }
        }

        private void SetCurrentView(string viewKey)
        {
            MenuList.ForEach((item) =>
            {
                if (item.Key == viewKey)
                {
                    CurrentView = item.ViewInstance == null ? item.ViewInstance = CreateViewInstance(item.Key) : item.ViewInstance;
                }
            });

            UpdateMenu();
        }

        private void UpdateMenu()
        {
            MenuList.ForEach((item) =>
            {
                item.IsSelected = item.ViewInstance == CurrentView;
            });
        }

        private object CreateViewInstance(string viewKey)
        {
            switch (viewKey)
            {
                case "LAUNCHER":
                    return new PageLauncherView();

                case "COMMUNITY":
                    return new PageCommunityView();

                case "ABOUT":
                    return new PageAboutView();

                default:
                    return null;
            }
        }

        // TODO: 和关于模块里的检查更新整合
        private void CheckUpdate()
        {
            if (!_ConfigManager.CheckUpdateOnStartup)
            {
                return;
            }

            HttpHelper.GetAsync(new RequestGetModel<AppInfoModel>
            {
                Path = "/api/v1/sgw2/info",
                Query = new Dictionary<string, string>
                {
                    { "version", Assembly.GetExecutingAssembly().GetName().Version.ToString() },
                },
                SuccessCallback = (res) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var appInfo = res.result;
                        var latestVersion = appInfo.version;
                        var latestVersionSrc = appInfo.setupSrc;
                        var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

                        if (UtilHelper.GetVersionWeight(latestVersion) <= UtilHelper.GetVersionWeight(version))
                        {
                            return;
                        }

                        var setupPackagePath = Path.Combine(GVar.Instance.PathManager.AppSetupPackageFolder, "StartGuildwars2-Setup-" + latestVersion + ".exe");

                        if (File.Exists(setupPackagePath))
                        {
                            UtilHelper.ShowConfirmDialog(new ConfirmDialogInterfaceModel
                            {
                                Content = "点击 “确定” 将重启应用以安装更新",
                                Title = "安装更新",
                                ConfirmCallback = () =>
                                {
                                    UtilHelper.InstallUpdate(setupPackagePath);
                                },
                            });
                        }
                        else
                        {
                            HttpHelper.DownloadFileAsync(new RequestDownloadFileModel
                            {
                                RemoteUrl = latestVersionSrc,
                                LocalPath = setupPackagePath,
                                SuccessCallback = () =>
                                {
                                    UtilHelper.ShowConfirmDialog(new ConfirmDialogInterfaceModel
                                    {
                                        Content = "点击 “确定” 将重启应用以安装更新",
                                        Title = "安装更新",
                                        ConfirmCallback = () =>
                                        {
                                            UtilHelper.InstallUpdate(setupPackagePath);
                                        },
                                    });
                                },
                            });
                        }
                    });
                },
            });
        }
    }
}
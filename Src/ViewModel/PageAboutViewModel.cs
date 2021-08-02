using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using StartGuildwars2.Global;
using StartGuildwars2.Helper;
using StartGuildwars2.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Navigation;

namespace StartGuildwars2.ViewModel
{
    public class PageAboutViewModel : ViewModelBase
    {
        private readonly ConfigManager _ConfigManager;

        public bool CheckUpdateOnStartup { get; private set; }
        public bool CheckAddonUpdateOnStartup { get; private set; }
        public string Version { get; private set; } = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public string LatestVersion { get; private set; } = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public string LatestVersionSrc { get; private set; }
        public string CheckUpdateText { get; private set; }
        public bool CheckUpdateLoading { get; private set; }
        public string InstallUpdateText { get; private set; }
        public bool InstallUpdateLoading { get; private set; }

        public RelayCommand<string> ToggleCheckUpdateOnStartupCommand => new Lazy<RelayCommand<string>>(() => new RelayCommand<string>(ToggleExitOnStartup)).Value;
        public RelayCommand<string> ToggleCheckAddonUpdateOnStartupCommand => new Lazy<RelayCommand<string>>(() => new RelayCommand<string>(ToggleCheckAddonUpdateOnStartup)).Value;
        public RelayCommand CheckUpdateCommand => new Lazy<RelayCommand>(() => new RelayCommand(CheckUpdate)).Value;
        public RelayCommand InstallUpdateCommand => new Lazy<RelayCommand>(() => new RelayCommand(InstallUpdate)).Value;
        public RelayCommand<RequestNavigateEventArgs> HyperlinkCommand => new Lazy<RelayCommand<RequestNavigateEventArgs>>(() => new RelayCommand<RequestNavigateEventArgs>(Hyperlink)).Value;
        public RelayCommand<RequestNavigateEventArgs> EmailCommand => new Lazy<RelayCommand<RequestNavigateEventArgs>>(() => new RelayCommand<RequestNavigateEventArgs>(Email)).Value;

        public PageAboutViewModel()
        {
            _ConfigManager = GVar.Instance.ConfigManager;
            CheckUpdateOnStartup = _ConfigManager.CheckUpdateOnStartup;
            CheckAddonUpdateOnStartup = _ConfigManager.CheckAddonUpdateOnStartup;
            CheckUpdate();
        }

        public bool HasNewerVersion
        {
            get
            {
                return UtilHelper.GetVersionWeight(LatestVersion) > UtilHelper.GetVersionWeight(Version);
            }
        }

        private void ToggleExitOnStartup(string IsChecked)
        {
            switch (IsChecked)
            {
                case "check":
                    _ConfigManager.SaveCheckUpdateOnStartup(true);
                    break;

                case "uncheck":
                    _ConfigManager.SaveCheckUpdateOnStartup(false);
                    break;
            }
        }

        private void ToggleCheckAddonUpdateOnStartup(string IsChecked)
        {
            switch (IsChecked)
            {
                case "check":
                    _ConfigManager.SaveCheckAddonUpdateOnStartup(true);
                    break;

                case "uncheck":
                    _ConfigManager.SaveCheckAddonUpdateOnStartup(false);
                    break;
            }
        }

        private void CheckUpdate()
        {
            CheckUpdateLoading = true;

            HttpHelper.GetAsync(new RequestGetModel<AppInfoModel>
            {
                Path = "/api/v1/sgw2/info",
                Query = new Dictionary<string, string>
                {
                    { "version", Assembly.GetExecutingAssembly().GetName().Version.ToString() },
                },
                SuccessCallback = (res) =>
                {
                    var appInfo = res.result;
                    LatestVersion = appInfo.version;
                    LatestVersionSrc = appInfo.setupSrc;

                    if (UtilHelper.GetVersionWeight(LatestVersion) <= UtilHelper.GetVersionWeight(Version))
                    {
                        CheckUpdateText = "当前是最新版本";
                    }
                },
                ErrorCallback = ex =>
                {
                    CheckUpdateText = "请求出错";
                },
                CompleteCallback = () =>
                {
                    new Thread(() =>
                    {
                        Thread.Sleep(UtilHelper.GetRandomNumber(1000, 2000));
                        CheckUpdateLoading = false;
                    }).Start();
                }
            });
        }

        private void InstallUpdate()
        {
            InstallUpdateLoading = true;

            var setupPackagePath = Path.Combine(GVar.Instance.PathManager.AppSetupPackageFolder, "StartGuildwars2-Setup-" + LatestVersion + ".exe");

            if (File.Exists(setupPackagePath))
            {
                InstallUpdateLoading = false;
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
                    RemoteUrl = LatestVersionSrc,
                    LocalPath = setupPackagePath,
                    SuccessCallback = () =>
                    {
                        InstallUpdateText = "";
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
                    ErrorCallback = ex =>
                    {
                        InstallUpdateText = "下载失败";
                    },
                    CompleteCallback = () =>
                    {
                        InstallUpdateLoading = false;
                    },
                });
            }
        }

        private void Hyperlink(RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
        }

        private void Email(RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
        }
    }
}
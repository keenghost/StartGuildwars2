using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using StartGuildwars2.Global;
using System;

namespace StartGuildwars2.ViewModel
{
    public class PageLauncherViewModel : ViewModelBase
    {
        private readonly ConfigManager _ConfigManager;

        public bool ExitOnStartup { get; private set; }

        public RelayCommand<string> ToggleExitOnStartupCommand => new Lazy<RelayCommand<string>>(() => new RelayCommand<string>(ToggleExitOnStartup)).Value;

        public PageLauncherViewModel()
        {
            _ConfigManager = GVar.Instance.ConfigManager;
            ExitOnStartup = _ConfigManager.ExitOnStartup;
        }

        private void ToggleExitOnStartup(string IsChecked)
        {
            switch (IsChecked)
            {
                case "check":
                    _ConfigManager.SaveExitOnStartup(true);
                    break;

                case "uncheck":
                    _ConfigManager.SaveExitOnStartup(false);
                    break;
            }
        }
    }
}
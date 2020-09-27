using GalaSoft.MvvmLight.Command;
using HandyControl.Interactivity;
using StartGuildwars2.Global;
using StartGuildwars2.Helper;
using StartGuildwars2.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace StartGuildwars2.ViewModel
{
    public class ComStartupArgumentsDialogViewModel : BaseDialogDataViewModel
    {
        private string GameType;
        private readonly ConfigManager _ConfigManager;

        public ObservableCollection<StartupArgumentModel> StartupArgumentList { get; set; }

        public RelayCommand<string> RemoveCommand => new Lazy<RelayCommand<string>>(() => new RelayCommand<string>(Remove)).Value;
        public RelayCommand AppendCommand => new Lazy<RelayCommand>(() => new RelayCommand(Append)).Value;
        public RelayCommand CloseCommand => new Lazy<RelayCommand>(() => new RelayCommand(Close)).Value;
        public RelayCommand SaveCommand => new Lazy<RelayCommand>(() => new RelayCommand(Save)).Value;

        public ComStartupArgumentsDialogViewModel()
        {
            _ConfigManager = GVar.Instance.ConfigManager;
        }

        public void Prepare(string type)
        {
            switch (type)
            {
                case "MF":
                    GameType = "MF";
                    StartupArgumentList = new ObservableCollection<StartupArgumentModel>(_ConfigManager.MFStartupArgumentList);
                    break;

                case "GF":
                    GameType = "GF";
                    StartupArgumentList = new ObservableCollection<StartupArgumentModel>(_ConfigManager.GFStartupArgumentList);
                    break;

                default:
                    break;
            }
        }

        private void Remove(string id)
        {
            var index = StartupArgumentList.IndexOf(StartupArgumentList.Where(item => item.ID == id).FirstOrDefault());

            if (index != -1)
            {
                StartupArgumentList.RemoveAt(index);
            }
        }

        private void Append()
        {
            StartupArgumentList.Add(new StartupArgumentModel()
            {
                ID = UtilHelper.GetUniqueID(),
                Command = "",
                Enable = true,
            });
        }

        private void Close()
        {
            ((ICommand)ControlCommands.Close).Execute(null);
        }

        private void Save()
        {
            var cleanStartupArguments = StartupArgumentList.Where(item => !string.IsNullOrEmpty(item.Command)).ToList();
            _ConfigManager.SaveStartupArgumentList(cleanStartupArguments, GameType);
            DialogCallback?.Invoke(true);
            ((ICommand)ControlCommands.Close).Execute(null);
        }
    }
}
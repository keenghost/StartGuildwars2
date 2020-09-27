using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using HandyControl.Interactivity;
using System;
using System.Windows.Input;

namespace StartGuildwars2.ViewModel
{
    public class BaseAlertDialogViewModel : ViewModelBase
    {
        public string Title { get; set; } = "提示";
        public string Content { get; set; } = "默认内容";
        public string CompleteButtonText { get; set; } = "关闭";
        public Action CompleteCallback { get; set; }

        public RelayCommand CompleteCommand => new Lazy<RelayCommand>(() => new RelayCommand(Complete)).Value;

        public BaseAlertDialogViewModel()
        {
        }

        private void Complete()
        {
            CompleteCallback?.Invoke();
            ((ICommand)ControlCommands.Close).Execute(null);
        }
    }
}
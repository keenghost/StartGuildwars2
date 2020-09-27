using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using HandyControl.Interactivity;
using System;
using System.Windows.Input;

namespace StartGuildwars2.ViewModel
{
    public class BaseConfirmDialogViewModel : ViewModelBase
    {
        public string Title { get; set; } = "提示";
        public string Content { get; set; } = "默认内容";
        public string ConfirmButtonText { get; set; } = "确定";
        public string CancelButtonText { get; set; } = "取消";
        public Action ConfirmCallback { get; set; }
        public Action CancelCallback { get; set; }
        public Action CompleteCallback { get; set; }
        public bool ShowClose { get; set; } = false;

        public RelayCommand ConfirmCommand => new Lazy<RelayCommand>(() => new RelayCommand(Confirm)).Value;
        public RelayCommand CancelCommand => new Lazy<RelayCommand>(() => new RelayCommand(Cancel)).Value;

        public BaseConfirmDialogViewModel()
        {
        }

        private void Confirm()
        {
            ConfirmCallback?.Invoke();
            CompleteCallback?.Invoke();
            ((ICommand)ControlCommands.Close).Execute(null);
        }

        private void Cancel()
        {
            CancelCallback?.Invoke();
            CompleteCallback?.Invoke();
            ((ICommand)ControlCommands.Close).Execute(null);
        }
    }
}
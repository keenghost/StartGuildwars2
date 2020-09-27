using GalaSoft.MvvmLight;
using System;

namespace StartGuildwars2.ViewModel
{
    public class BaseDialogDataViewModel : ViewModelBase
    {
        public Action<object> DialogCallback { get; set; }

        public BaseDialogDataViewModel()
        {
        }
    }
}
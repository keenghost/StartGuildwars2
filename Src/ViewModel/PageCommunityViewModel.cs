using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using StartGuildwars2.Helper;
using StartGuildwars2.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;

namespace StartGuildwars2.ViewModel
{
    public class PageCommunityViewModel : ViewModelBase
    {
        public ObservableCollection<CommunityCategoryModel> CommunityList { get; private set; }
        public bool Loading { get; private set; } = true;
        public bool ReloadFreezing { get; private set; } = false;

        public RelayCommand ReloadCommand => new Lazy<RelayCommand>(() => new RelayCommand(Reload)).Value;
        public RelayCommand<string> HyperlinkCommand => new Lazy<RelayCommand<string>>(() => new RelayCommand<string>(Hyperlink)).Value;

        public PageCommunityViewModel()
        {
            GetList();
        }

        private void Reload()
        {
            GetList();
            FreezeReload();
        }

        private void Hyperlink(string uri)
        {
            Process.Start(uri);
        }

        private void GetList()
        {
            Loading = true;

            HttpHelper.GetAsync(new RequestGetModel<List<CommunityCategoryModel>>
            {
                Path = "/api/v1/sgw2/community",
                SuccessCallback = res =>
                {
                    CommunityList = new ObservableCollection<CommunityCategoryModel>(res.result);
                },
                CompleteCallback = () =>
                {
                    Loading = false;
                },
            });
        }

        private void FreezeReload()
        {
            ReloadFreezing = true;

            new Thread(() =>
            {
                Thread.Sleep(UtilHelper.GetRandomNumber(3000, 4000));
                ReloadFreezing = false;
            }).Start();
        }
    }
}
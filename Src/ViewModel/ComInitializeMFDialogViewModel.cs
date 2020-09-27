using GalaSoft.MvvmLight.Command;
using HandyControl.Interactivity;
using Microsoft.Win32;
using StartGuildwars2.Global;
using StartGuildwars2.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management;
using System.Windows.Input;

namespace StartGuildwars2.ViewModel
{
    public class ComInitializeMFDialogViewModel : BaseDialogDataViewModel
    {
        private static readonly string pickUserPlaceholder = "当前无用户可选，请新建用户";
        private readonly ConfigManager _ConfigManager;

        public string MFPath { get; set; }
        public bool UserType { get; set; } = false;
        public List<string> AllUsernameList { get; private set; } = new List<string>();
        public ObservableCollection<string> UsernameList { get; private set; } = new ObservableCollection<string>();
        public string PickUsername { get; set; } = pickUserPlaceholder;
        public string NewUsername { get; set; } = "";
        public string Password { get; set; } = "";

        public RelayCommand PickMFPathCommand => new Lazy<RelayCommand>(() => new RelayCommand(PickMFPath)).Value;
        public RelayCommand<string> ChangeUserTypeCommad => new Lazy<RelayCommand<string>>(() => new RelayCommand<string>(ChangeUserType)).Value;
        public RelayCommand FinishCommand => new Lazy<RelayCommand>(() => new RelayCommand(Finish)).Value;

        public ComInitializeMFDialogViewModel()
        {
            _ConfigManager = GVar.Instance.ConfigManager;
            UsernameList.Add(pickUserPlaceholder);
            SetUserList();
        }

        private void PickMFPath()
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

                if (!string.IsNullOrEmpty(_ConfigManager.GFPath))
                {
                    if (_ConfigManager.GFPath.StartsWith(directory, StringComparison.InvariantCultureIgnoreCase))
                    {
                        UtilHelper.ShowAlertDialog(new Model.AlertDialogInterfaceModel { Content = "国服已选择此路径，请重新选择其它路径" });
                        return;
                    }
                }

                MFPath = filepath;
            }
        }

        private void ChangeUserType(string type)
        {
            UserType = type == "NEW";
        }

        private void Finish()
        {
            if (string.IsNullOrEmpty(MFPath))
            {
                UtilHelper.ShowAlertDialog(new Model.AlertDialogInterfaceModel { Content = "未选择美服游戏路径" });
                return;
            }

            if (!UserType)
            {
                if (string.IsNullOrEmpty(PickUsername) || PickUsername.Equals(pickUserPlaceholder))
                {
                    UtilHelper.ShowAlertDialog(new Model.AlertDialogInterfaceModel { Content = "请选择用户或新建用户" });
                    return;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(NewUsername))
                {
                    UtilHelper.ShowAlertDialog(new Model.AlertDialogInterfaceModel { Content = "请输入新用户名" });
                    return;
                }

                if (AllUsernameList.IndexOf(NewUsername) != -1)
                {
                    UtilHelper.ShowAlertDialog(new Model.AlertDialogInterfaceModel { Content = "用户名已存在，或者是系统保留用户名" });
                    return;
                }
            }

            if (string.IsNullOrEmpty(Password))
            {
                UtilHelper.ShowAlertDialog(new Model.AlertDialogInterfaceModel { Content = "请输入密码" });
                return;
            }

            if (!UserType)
            {
                if (UtilHelper.VerifySystemUser(PickUsername, Password))
                {
                    try
                    {
                        UtilHelper.CheckSystemUserFolderAndCreate(PickUsername, Password);
                    }
                    catch (Exception e)
                    {
                        UtilHelper.ShowAlertDialog(new Model.AlertDialogInterfaceModel { Content = "生成用户文件夹失败:\r\n" + e.Message });
                        return;
                    }

                    _ConfigManager.SaveMFUser(PickUsername, Password);
                }
                else
                {
                    UtilHelper.ShowAlertDialog(new Model.AlertDialogInterfaceModel { Content = "密码不正确" });
                    return;
                }
            }
            else
            {
                try
                {
                    UtilHelper.CreateSystemUser(NewUsername, Password);
                }
                catch (Exception e)
                {
                    UtilHelper.ShowAlertDialog(new Model.AlertDialogInterfaceModel { Content = "创建新用户失败:\r\n" + e.Message });
                    return;
                }

                try
                {
                    UtilHelper.CheckSystemUserFolderAndCreate(NewUsername, Password);
                }
                catch (Exception e)
                {
                    UtilHelper.ShowAlertDialog(new Model.AlertDialogInterfaceModel { Content = "生成用户文件夹失败:\r\n" + e.Message });
                    return;
                }

                _ConfigManager.SaveMFUser(NewUsername, Password);
            }

            _ConfigManager.SaveGamePath(MFPath, "MF");
            DialogCallback?.Invoke(true);
            ((ICommand)ControlCommands.Close).Execute(null);
        }

        private void SetUserList()
        {
            ManagementObjectSearcher usersSearcher = new ManagementObjectSearcher(@"SELECT * FROM Win32_UserAccount");
            ManagementObjectCollection users = usersSearcher.Get();

            var rawAllUsers = users.Cast<ManagementObject>();
            var rawActiveUsers = rawAllUsers.Where(u =>
            {
                return (bool)u["LocalAccount"] == true && (bool)u["Disabled"] == false && (bool)u["Lockout"] == false && int.Parse(u["SIDType"].ToString()) == 1 && u["Name"].ToString() != "HomeGroupUser$";
            });

            var allUsers = (from item in rawAllUsers select item["Name"].ToString()).ToList();
            var activeUsers = (from item in rawActiveUsers select item["Name"].ToString()).ToList();

            AllUsernameList.AddRange(allUsers);

            if (activeUsers.Count < 2)
            {
                return;
            }

            UsernameList.Clear();
            PickUsername = "";

            var username = Environment.UserName;

            foreach (var activeUser in activeUsers)
            {
                if (activeUser != username)
                {
                    UsernameList.Add(activeUser);
                }
            }
        }
    }
}
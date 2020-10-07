using HandyControl.Controls;
using HandyControl.Tools.Extension;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StartGuildwars2.Model;
using StartGuildwars2.View;
using StartGuildwars2.ViewModel;
using System;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text.RegularExpressions;

namespace StartGuildwars2.Helper
{
    public class UtilHelper
    {
        [DllImport("kernel32.dll")]
        private static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, int dwFlags);

        private static readonly long timeZero = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;

        public static int GetRandomNumber(int start, int end)
        {
            return new Random().Next(start, end);
        }

        public static string GetTimestampNow()
        {
            return ((DateTime.UtcNow.Ticks - timeZero) / 10000000000L).ToString();
        }

        public static string GetUniqueID()
        {
            var random = new Random();

            return GetTimestampNow() + random.Next(100000, 999999).ToString();
        }

        public static JObject ReadJsonFile(string filepath)
        {
            try
            {
                StreamReader streamReader = File.OpenText(filepath);
                JsonTextReader jsonTextReader = new JsonTextReader(streamReader);
                JObject jsonObject = (JObject)JToken.ReadFrom(jsonTextReader);
                streamReader.Close();

                return jsonObject;
            }
            catch
            {
                return new JObject();
            }
        }

        public static bool WriteJsonFile(string filepath, JObject jObject)
        {
            try
            {
                File.WriteAllText(filepath, JsonConvert.SerializeObject(jObject, Formatting.Indented));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void CreateSystemUser(string username, string password)
        {
            PrincipalContext context = new PrincipalContext(ContextType.Machine);
            UserPrincipal user = new UserPrincipal(context)
            {
                PasswordNotRequired = false,
                DisplayName = username,
                Name = username,
                Description = "user account for starting guildwars2",
                UserCannotChangePassword = true,
                PasswordNeverExpires = true,
                AccountExpirationDate = null,
            };

            user.SetPassword(password);
            user.Save();

            GroupPrincipal group = GroupPrincipal.FindByIdentity(context, "Users");
            group.Members.Add(user);
            group.Save();
        }

        public static void RemoveSystemUser(string username)
        {
            var ctx = new PrincipalContext(ContextType.Machine);
            var up = UserPrincipal.FindByIdentity(ctx, username);

            if (up != null)
            {
                up.Delete();
            }
        }

        public static bool RemoveAndCreateDirectoryLink(string sourceDirectory, string targetDirectory)
        {
            if (Directory.Exists(sourceDirectory))
            {
                try
                {
                    Directory.Delete(sourceDirectory, true);
                }
                catch
                {
                    return false;
                }
            }

            return CreateSymbolicLink(sourceDirectory, targetDirectory, 1);
        }

        public static bool VerifySystemUser(string username, string password)
        {
            try
            {
                PrincipalContext context = new PrincipalContext(ContextType.Machine);

                return context.ValidateCredentials(username, password);
            }
            catch
            {
                return false;
            }
        }

        public static void CheckSystemUserFolderAndCreate(string username, string password)
        {
            if (!Directory.Exists(Path.Combine("C:\\Users", username)))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    UserName = username,
                    Password = ConvertStringToSecureString(password),
                    FileName = "cmd.exe",
                    UseShellExecute = false,
                    LoadUserProfile = true,
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                };

                var p = Process.Start(startInfo);
                p.StandardInput.WriteLine("exit");
                p.WaitForExit();
            }
        }

        public static SecureString ConvertStringToSecureString(string source)
        {
            SecureString secureString = new SecureString();
            Array.ForEach(source.ToCharArray(), secureString.AppendChar);

            return secureString;
        }

        public static ulong GetVersionWeight(string version)
        {
            string pattern = @"^(\d+).(\d+).(\d+).(\d+)$";
            ulong w = 0;
            ulong x = 0;
            ulong y = 0;
            ulong z = 0;

            var match = Regex.Match(version, pattern);

            if (match.Success)
            {
                w = ulong.Parse(match.Result("$1"));
                x = ulong.Parse(match.Result("$2"));
                y = ulong.Parse(match.Result("$3"));
                z = ulong.Parse(match.Result("$4"));
            }

            return z + y * 100 + x * 1000000 + w * 10000000000;
        }

        public static string GetExeFileVersion(string filepath)
        {
            var defaultVersion = "0.0.0.0";

            if (File.Exists(filepath))
            {
                try
                {
                    var fileVersionInfo = FileVersionInfo.GetVersionInfo(filepath);
                    defaultVersion = string.Format("{0}.{1}.{2}.{3}", fileVersionInfo.FileMajorPart, fileVersionInfo.FileMinorPart, fileVersionInfo.FileBuildPart, fileVersionInfo.FilePrivatePart);
                }
                catch { }
            }

            return defaultVersion;
        }

        public static void ShowConfirmDialog(ConfirmDialogInterfaceModel config)
        {
            Dialog.Show<BaseConfirmDialogView>("DialogContainer").Initialize<BaseConfirmDialogViewModel>((vm) =>
            {
                if (!string.IsNullOrEmpty(config.Title))
                {
                    vm.Title = config.Title;
                }

                vm.Content = config.Content;
                vm.ConfirmButtonText = config.ConfirmButtonText;
                vm.CancelButtonText = config.CancelButtonText;
                vm.ConfirmCallback = config.ConfirmCallback;
                vm.CancelCallback = config.CancelCallback;
                vm.CompleteCallback = config.CompleteCallback;
                vm.ShowClose = config.ShowClose;
            });
        }

        public static void ShowAlertDialog(AlertDialogInterfaceModel config)
        {
            Dialog.Show<BaseAlertDialogView>("DialogContainer").Initialize<BaseAlertDialogViewModel>((vm) =>
            {
                if (!string.IsNullOrEmpty(config.Title))
                {
                    vm.Title = config.Title;
                }

                vm.Content = config.Content;
                vm.CompleteButtonText = config.CompleteButtonText;
                vm.CompleteCallback = config.CompleteCallback;
            });
        }

        public static void InstallUpdate(string setupPackagePath)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = setupPackagePath,
                Arguments = "/sp- /silent /norestart",
                UseShellExecute = false,
            });
        }
    }
}
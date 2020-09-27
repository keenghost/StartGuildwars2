using Ionic.Zip;
using System.IO;

namespace StartGuildwars2.Helper
{
    public class IOHelper
    {
        private static void CopyDirectory(string source, string target)
        {
            Directory.CreateDirectory(target);

            var files = Directory.GetFiles(source);

            foreach (var file in files)
            {
                var filename = Path.GetFileName(file);
                var newTarget = Path.Combine(target, filename);
                File.Copy(file, newTarget, true);
            }

            var folders = Directory.GetDirectories(source);

            foreach (var folder in folders)
            {
                var filename = Path.GetFileName(folder);
                var newTarget = Path.Combine(target, filename);
                CopyDirectory(folder, newTarget);
            }
        }

        public static void DeleteFileOrDirectory(string filepath)
        {
            if (Directory.Exists(filepath))
            {
                Directory.Delete(filepath, true);
                return;
            }

            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }
        }

        public static void ExtractZipFile(string sourceFilePath, string destFolderPath)
        {
            var z = ZipFile.Read(sourceFilePath);

            foreach (var e in z.Entries)
            {
                e.Extract(destFolderPath, ExtractExistingFileAction.OverwriteSilently);
            }
        }

        public static void MoveFileOrDirectory(string source, string target)
        {
            var isSameVolume = string.Compare(source.Substring(0, 1), target.Substring(0, 1), true) == 0;

            if (File.Exists(source))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(target));

                if (isSameVolume)
                {
                    File.Move(source, target);
                }
                else
                {
                    File.Copy(source, target, true);
                    DeleteFileOrDirectory(source);
                }

                return;
            }

            if (!Directory.Exists(source))
            {
                return;
            }

            if (isSameVolume && !Directory.Exists(target))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(target));
                Directory.Move(source, target);
            }
            else
            {
                CopyDirectory(source, target);
                DeleteFileOrDirectory(source);
            }
        }

        public static bool IsDirectoryLnk(string filepath)
        {
            if (!Directory.Exists(filepath))
            {
                return false;
            }

            try
            {
                FileInfo pathInfo = new FileInfo(filepath);

                return pathInfo.Attributes.HasFlag(FileAttributes.ReparsePoint);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsDirectoryEmpty(string filepath)
        {
            if (!Directory.Exists(filepath))
            {
                return true;
            }

            return !(Directory.GetDirectories(filepath).Length > 0 || Directory.GetFiles(filepath).Length > 0);
        }
    }
}
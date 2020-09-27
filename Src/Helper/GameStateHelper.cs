using StartGuildwars2.Global;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace StartGuildwars2.Helper
{
    public class GameStateHelper
    {
        public static string ProcessNameX86 = "Gw2";
        public static string ProcessNameX64 = "Gw2-64";

        public static List<Process> GetCombinedProcesses()
        {
            var processesX86 = Process.GetProcessesByName(ProcessNameX86);
            var processesX64 = Process.GetProcessesByName(ProcessNameX64);
            var list = new List<Process>();

            list.AddRange(processesX86);
            list.AddRange(processesX64);

            return list;
        }

        public static bool HasUnknownRunningGame(List<string> knownList)
        {
            var processes = GetCombinedProcesses();

            foreach (Process p in processes)
            {
                var inFlag = false;

                foreach (string knownPath in knownList)
                {
                    if (p.MainModule.FileName.Equals(knownPath, StringComparison.InvariantCultureIgnoreCase))
                    {
                        inFlag = true;
                    }
                }

                if (!inFlag)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool CheckRunningGame(string filepath)
        {
            var processes = GetCombinedProcesses();

            foreach (Process p in processes)
            {
                if (p.MainModule.FileName.Equals(filepath, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool KillMutant()
        {
            var processes = GetCombinedProcesses();

            if (processes.Count == 0)
            {
                return true;
            }

            try
            {
                var process = processes[0];

                var handleFindProcess = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = GVar.Instance.PathManager.ToolHandle64FilePath,
                        Arguments = "-p " + process.Id + " -a \"\\Sessions\\1\\BaseNamedObjects\\AN-Mutex-Window-Guild Wars 2\"",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                    },
                };

                handleFindProcess.Start();
                handleFindProcess.WaitForExit();
                var handleFindProcessOutput = handleFindProcess.StandardOutput.ReadToEnd();
                var matchMutant = Regex.Match(handleFindProcessOutput, @"(.*)Mutant(.*):(.*)");

                if (matchMutant.Success)
                {
                    var mutant = matchMutant.Result("$2").Trim();

                    var handleKillProcess = new Process()
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = GVar.Instance.PathManager.ToolHandle64FilePath,
                            Arguments = "-p " + process.Id + " -c " + mutant + " -y",
                            CreateNoWindow = true,
                            UseShellExecute = false,
                        }
                    };

                    handleKillProcess.Start();
                    handleKillProcess.WaitForExit();

                    return true;
                }
                else
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
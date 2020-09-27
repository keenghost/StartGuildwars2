using System.Collections.Generic;

namespace StartGuildwars2.Model
{
    public class AddonItemModel
    {
        public string _id { get; set; }
        public string name { get; set; }
        public string displayName { get; set; }
        public string description { get; set; }
        public string version { get; set; }
        public string limitExeVersion { get; set; }
        public string updateRequiredLastVersion { get; set; }
        public string src { get; set; }
        public string website { get; set; }
        public string mainDll { get; set; }
        public List<string> dependency { get; set; } = new List<string>();
        public List<string> conflict { get; set; } = new List<string>();
        public List<string> uninstall { get; set; } = new List<string>();
        public bool outdated { get; set; }
        public bool zh { get; set; }
    }

    public class InstalledAddonItemModel
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string MainDll { get; set; }
        public List<string> Uninstall { get; set; } = new List<string>();
    }

    public class DisplayAddonItemModel
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string Website { get; set; }
        public bool IsZh { get; set; }
        public bool IsInstalled { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanInstall { get; set; }
        public string ConflictDescription { get; set; }
    }

    public class AddonStepModel
    {
        public string Action { get; set; }
        public string AddonName { get; set; }
    }

    public class AddonProgressMessageModel
    {
        public string Type { get; set; }
        public List<AddonItemModel> Addons { get; set; } = new List<AddonItemModel>();
        public List<AddonStepModel> AddonSteps { get; set; } = new List<AddonStepModel>();
    }
}
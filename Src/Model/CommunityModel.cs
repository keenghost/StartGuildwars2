using System.Collections.Generic;

namespace StartGuildwars2.Model
{
    public class CommunityCategoryModel
    {
        public string category { get; set; }
        public List<CommunityItemModel> list { get; set; } = new List<CommunityItemModel>();
    }

    public class CommunityItemModel
    {
        public string name { get; set; }
        public string description { get; set; }
        public string website { get; set; }
        public bool recommend { get; set; }
        public List<CommunityItemTagModel> tags { get; set; } = new List<CommunityItemTagModel>();
    }

    public class CommunityItemTagModel
    {
        public string content { get; set; }
    }
}
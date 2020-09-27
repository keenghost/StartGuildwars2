using GalaSoft.MvvmLight;

namespace StartGuildwars2.Model
{
    public class StartupArgumentModel : ObservableObject
    {
        public string ID { get; set; } = "";
        public string Command { get; set; } = "";
        public bool Enable { get; set; } = false;
    }
}
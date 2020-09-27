using GalaSoft.MvvmLight;

namespace StartGuildwars2.Global
{
    public class GVar : ObservableObject
    {
        private GVar()
        {
        }

        private static readonly GVar instance = new GVar();

        public static GVar Instance
        {
            get
            {
                return instance;
            }
        }

        public PathManager PathManager { get; set; }
        public ConfigManager ConfigManager { get; set; }
    }
}
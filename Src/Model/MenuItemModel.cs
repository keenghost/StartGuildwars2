using GalaSoft.MvvmLight;

namespace StartGuildwars2.Model
{
    public class MenuItemModel : ObservableObject
    {
        public object ViewInstance { get; set; }
        public string Title { get; set; }
        public string Key { get; set; }
        public bool IsSelected { get; set; }
        public object Icon { get; set; }
    }
}
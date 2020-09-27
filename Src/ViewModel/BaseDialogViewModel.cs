using GalaSoft.MvvmLight;

namespace StartGuildwars2.ViewModel
{
    public class BaseDialogViewModel : ViewModelBase
    {
        public string DialogTitle { get; set; } = "默认标题";
        public bool DialogShowClose { get; set; } = true;
        public string DialogSize { get; set; } = "Large";
        public int Height { get; set; } = 480;

        public BaseDialogViewModel()
        {
        }

        public int Width
        {
            get
            {
                switch (DialogSize)
                {
                    case "Large":
                        return 720;

                    case "Middle":
                        return 600;

                    case "Small":
                        return 480;

                    default:
                        return 720;
                }
            }
        }
    }
}
using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using Ninject;

namespace StartGuildwars2.ViewModel
{
    public class ViewModelLocator
    {
        public IKernel Kernel { get; set; }

        public ViewModelLocator()
        {
            Kernel = new StandardKernel();
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<MainWindowViewModel>();
            SimpleIoc.Default.Register<PageLauncherViewModel>();
            SimpleIoc.Default.Register<PageCommunityViewModel>();
            SimpleIoc.Default.Register<PageAboutViewModel>();
        }

        public MainWindowViewModel Main => ServiceLocator.Current.GetInstance<MainWindowViewModel>();
        public PageLauncherViewModel PageLauncher => ServiceLocator.Current.GetInstance<PageLauncherViewModel>();
        public PageCommunityViewModel PageCommunity => ServiceLocator.Current.GetInstance<PageCommunityViewModel>();
        public PageAboutViewModel PageAbout => ServiceLocator.Current.GetInstance<PageAboutViewModel>();
        public ComLaunchGamePanelViewModel ComLaunchGamePanel => Kernel.Get<ComLaunchGamePanelViewModel>();
        public ComStartupArgumentsDialogViewModel ComStartupArgumentsDialog => Kernel.Get<ComStartupArgumentsDialogViewModel>();
        public ComInitializeMFDialogViewModel ComInitializeMFDialog => Kernel.Get<ComInitializeMFDialogViewModel>();
        public BaseDialogViewModel BaseDialog => Kernel.Get<BaseDialogViewModel>();
        public BaseConfirmDialogViewModel BaseConfirmDialog => Kernel.Get<BaseConfirmDialogViewModel>();
        public BaseAlertDialogViewModel BaseAlertDialog => Kernel.Get<BaseAlertDialogViewModel>();
        public ComAddonsDialogViewModel ComAddonsDialog => Kernel.Get<ComAddonsDialogViewModel>();
        public ComAddonsProgressDialogViewModel ComAddonsProgressDialog => Kernel.Get<ComAddonsProgressDialogViewModel>();

        public static void Cleanup()
        {
            // TODO: Clear the ViewModels
        }
    }
}
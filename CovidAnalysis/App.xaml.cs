using CovidAnalysis.Pages;
using CovidAnalysis.Services.CountryService;
using CovidAnalysis.Services.LogEntryService;
using CovidAnalysis.Services.Repository;
using CovidAnalysis.Services.StreamDownloader;
using CovidAnalysis.ViewModels;
using Prism;
using Prism.Ioc;
using Prism.Plugin.Popups;
using Prism.Unity;
using Xamarin.Forms;

namespace CovidAnalysis
{
    public partial class App : PrismApplication
    {
        public App(IPlatformInitializer platformInitializer = null) : base(platformInitializer) { }

        protected override async void OnInitialized()
        {
            InitializeComponent();

            await NavigationService.NavigateAsync($"{nameof(NavigationPage)}/{nameof(HomePage)}");
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // navigation
            containerRegistry.RegisterForNavigation<NavigationPage>();

            containerRegistry.RegisterPopupNavigationService();

            containerRegistry.RegisterForNavigation<HomePage, HomePageViewModel>();
            containerRegistry.RegisterForNavigation<SelectOnePopupPage, SelectOnePopupPageViewModel>();

            // services
            containerRegistry.RegisterInstance<IRepository>(Container.Resolve<Repository>());
            containerRegistry.RegisterInstance<IStreamDownloader>(Container.Resolve<StreamDownloader>());
            containerRegistry.RegisterInstance<ILogEntryService>(Container.Resolve<LogEntryService>());
            containerRegistry.RegisterInstance<ICountryService>(Container.Resolve<CountryService>());
        }
    }
}

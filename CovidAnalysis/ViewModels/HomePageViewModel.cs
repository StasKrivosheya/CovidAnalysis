using Prism.Navigation;

namespace CovidAnalysis.ViewModels
{
    public class HomePageViewModel : BaseViewModel
    {
        public HomePageViewModel(INavigationService navigationService) : base(navigationService)
        {
        }

        #region -- Overrides --

        public override void Initialize(INavigationParameters parameters)
        {
            base.Initialize(parameters);

            Title = "Home page";
        }

        #endregion
    }
}

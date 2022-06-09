using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CovidAnalysis.Models.CountryItem;
using Prism.Navigation;

namespace CovidAnalysis.ViewModels
{
    public class SelectOnePopupPageViewModel : BaseViewModel
    {
        private IEnumerable<CountryItemModel> _items;

        public SelectOnePopupPageViewModel(INavigationService navigationService)
            : base(navigationService)
        {
        }

        #region -- Public properties --

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        private string _selectedCountry;
        public string SelectedCountry
        {
            get => _selectedCountry;
            set => SetProperty(ref _selectedCountry, value);
        }

        private ObservableCollection<string> _showableItems;
        public ObservableCollection<string> ShowableItems
        {
            get => _showableItems;
            set => SetProperty(ref _showableItems, value);
        }

        #endregion

        #region -- Overrides --

        public override void Initialize(INavigationParameters parameters)
        {
            base.Initialize(parameters);

            if (parameters.TryGetValue(Constants.Navigation.COLLECTION_FOR_SELECTION, out IEnumerable<CountryItemModel> countries)
                && countries is not null)
            {
                _items = countries.OrderBy(country => country.CountryName).ToList();;

                ShowableItems = new(_items.Select(country => country.CountryName));
            }
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            base.OnPropertyChanged(args);

            if (args.PropertyName is nameof(SearchText)
                && _items is not null)
            {
                ShowableItems = string.IsNullOrWhiteSpace(SearchText)
                    ? new(_items.Select(i => i.CountryName))
                    : new(_items.Where(i =>
                        i.CountryName.ToLower().Contains(SearchText.ToLower())
                        || i.IsoCode.ToLower().Contains(SearchText.ToLower())
                    ).Select(j => j.CountryName));
            }
            else if (args.PropertyName is nameof(SelectedCountry))
            {
                SearchText = SelectedCountry;
                ShowableItems = new();

                var countryModel = _items.FirstOrDefault(c => c.CountryName == SelectedCountry);

                var parameters = new NavigationParameters
                {
                    { Constants.Navigation.SELECTED_COUNTRY, countryModel }
                };

                NavigationService.GoBackAsync(parameters);
            }
        }

        #endregion
    }
}

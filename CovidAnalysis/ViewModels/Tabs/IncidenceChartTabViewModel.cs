using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using CovidAnalysis.Extensions;
using CovidAnalysis.Models.CountryItem;
using CovidAnalysis.Pages;
using CovidAnalysis.Services.CountryService;
using CovidAnalysis.Services.LogEntryService;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;

namespace CovidAnalysis.ViewModels.Tabs
{
    public class IncidenceChartTabViewModel : BaseViewModel
    {
        private readonly ILogEntryService _logEntryService;
        private readonly ICountryService _countryService;
        private readonly IPageDialogService _pageDialogService;

        // if equals 1 - selecting 1st country, 2 - 2nd
        // 0 stands for no pending selection
        private int _selectingCountryNumber;

        public IncidenceChartTabViewModel(
            INavigationService navigationService,
            ILogEntryService logEntryService,
            ICountryService countryService,
            IPageDialogService pageDialogService)
            : base(navigationService)
        {
            _logEntryService = logEntryService;
            _countryService = countryService;
            _pageDialogService = pageDialogService;
        }

        #region -- Public properties --

        public List<string> LogEntryPropertyNames => Constants.LOG_ENTRY_PROPERTY_NAMES;

        private string _selectedLogEntryProperty = Constants.LOG_ENTRY_PROPERTY_NAMES.First();
        public string SelectedLogEntryProperty
        {
            get => _selectedLogEntryProperty;
            set => SetProperty(ref _selectedLogEntryProperty, value);
        }

        private bool _shouldShowRawData = true;
        public bool ShouldShowRawData
        {
            get => _shouldShowRawData;
            set => SetProperty(ref _shouldShowRawData, value);
        }

        private bool _shouldShowSmoothedData;
        public bool ShouldShowSmoothedData
        {
            get => _shouldShowSmoothedData;
            set => SetProperty(ref _shouldShowSmoothedData, value);
        }

        private PlotModel _incidenceChartPlotModel;
        public PlotModel IncidenceChartPlotModel
        {
            get => _incidenceChartPlotModel;
            set => SetProperty(ref _incidenceChartPlotModel, value);
        }

        // pre-initialization is needed for correct work of TargetNullValue when binding to .CountryName in xaml
        private CountryItemModel _firstSelectedCountry = new();
        public CountryItemModel FirstSelectedCountry
        {
            get => _firstSelectedCountry;
            set => SetProperty(ref _firstSelectedCountry, value);
        }

        private CountryItemModel _secondSelectedCountry = new();
        public CountryItemModel SecondSelectedCountry
        {
            get => _secondSelectedCountry;
            set => SetProperty(ref _secondSelectedCountry, value);
        }

        private ICommand _selectCountryCommand;
        public ICommand SelectCountryCommand => _selectCountryCommand ??= new DelegateCommand<string>(async (parameter) => await OnSelectCountryCommandAsync(parameter));

        #endregion

        #region -- Overrides --

        public override void Initialize(INavigationParameters parameters)
        {
            base.Initialize(parameters);
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);

            if (parameters.TryGetValue(Constants.Navigation.SELECTED_COUNTRY, out CountryItemModel selectedCountry))
            {
                switch (_selectingCountryNumber)
                {
                    case 1:
                        FirstSelectedCountry = selectedCountry;
                        break;
                    case 2:
                        SecondSelectedCountry = selectedCountry;
                        break;
                    default:
                        _selectingCountryNumber = default;
                        break;
                }
            }
        }

        protected override async void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            base.OnPropertyChanged(args);

            if (args.PropertyName is nameof(FirstSelectedCountry)
                                    or nameof(SecondSelectedCountry)
                                    or nameof(ShouldShowRawData)
                                    or nameof(ShouldShowSmoothedData)
                                    or nameof(SelectedLogEntryProperty))
            {
                await DispayMortalityAsync(FirstSelectedCountry, SecondSelectedCountry);
            }
        }

        #endregion

        #region -- Private helpers --

        private async Task OnSelectCountryCommandAsync(string parameter)
        {
            if (int.TryParse(parameter, out int countryNumber))
            {
                _selectingCountryNumber = countryNumber;

                var countries = await _countryService.GetCountriesListAsync();

                var parameters = new NavigationParameters
                {
                    { Constants.Navigation.COLLECTION_FOR_SELECTION, countries },
                };

                await NavigationService.NavigateAsync(nameof(SelectOnePopupPage), parameters);
            }
        }

        public async Task DispayMortalityAsync(CountryItemModel country1, CountryItemModel country2)
        {
            if (country1?.IsoCode is null || country2?.IsoCode is null) return;

            var country1Entries = await _logEntryService.GetEntriesListAsync(e => e.IsoCode == country1.IsoCode);
            var country2Entries = await _logEntryService.GetEntriesListAsync(e => e.IsoCode == country2.IsoCode);

            country1Entries = country1Entries.OrderBy(e => e.Date).ToList();
            country2Entries = country2Entries.OrderBy(e => e.Date).ToList();

            var plotModel = new PlotModel
            {
                Title = SelectedLogEntryProperty,
                LegendTitle = "Legend",
                LegendPosition = LegendPosition.LeftTop,
            };

            var xAxis = new LinearAxis
            {
                Title = "Date",
                Position = AxisPosition.Bottom,
                LabelFormatter = (param) => DateTime.FromOADate(param).ToString("MMM, yyyy")
            };
            plotModel.Axes.Add(xAxis);

            var yAxis = new LinearAxis
            {
                Title = "Values",
                Position = AxisPosition.Left
            };
            plotModel.Axes.Add(yAxis);

            var selectedPropertyName = Regex.Replace(SelectedLogEntryProperty, @"\s+", "");

            if (ShouldShowRawData)
            {
                LineSeries country1RawSeries = new()
                {
                    Title = $"{SelectedLogEntryProperty}, {country1.IsoCode}",
                };

                LineSeries country2RawSeries = new()
                {
                    Title = $"{SelectedLogEntryProperty}, {country2.IsoCode}",
                };

                foreach (var entry in country1Entries)
                {
                    country1RawSeries.Points.Add(new DataPoint(entry.Date.ToOADate(), entry.GetPropertyValueByName(selectedPropertyName)));
                }

                foreach (var entry in country2Entries)
                {
                    country2RawSeries.Points.Add(new DataPoint(entry.Date.ToOADate(), entry.GetPropertyValueByName(selectedPropertyName)));
                }

                plotModel.Series.Add(country1RawSeries);
                plotModel.Series.Add(country2RawSeries);
            }

            if (ShouldShowSmoothedData)
            {
                LineSeries country1SmoothedSeries = new()
                {
                    Title = $"{SelectedLogEntryProperty} SMOOTHED, {country1.IsoCode}",
                };

                LineSeries country2SmoothedSeries = new()
                {
                    Title = $"{SelectedLogEntryProperty} SMOOTHED, {country2.IsoCode}",
                };

                var country1SicknessSmoothed = country1Entries.Select(u => u.GetPropertyValueByName(selectedPropertyName)).GetSmoothed(7).ToList();
                for (int i = 0; i < country1Entries.Count - 1; i++)
                {
                    country1Entries[i].SetPropertyValueByName(selectedPropertyName, country1SicknessSmoothed[i]);
                }
                foreach (var entry in country1Entries)
                {
                    country1SmoothedSeries.Points.Add(new DataPoint(entry.Date.ToOADate(), entry.GetPropertyValueByName(selectedPropertyName)));
                }

                var country2SicknessSmoothed = country2Entries.Select(u => u.GetPropertyValueByName(selectedPropertyName)).GetSmoothed(7).ToList();
                for (int i = 0; i < country2Entries.Count - 1; i++)
                {
                    country2Entries[i].SetPropertyValueByName(selectedPropertyName, country2SicknessSmoothed[i]);
                }
                foreach (var entry in country2Entries)
                {
                    country2SmoothedSeries.Points.Add(new DataPoint(entry.Date.ToOADate(), entry.GetPropertyValueByName(selectedPropertyName)));
                }

                plotModel.Series.Add(country1SmoothedSeries);
                plotModel.Series.Add(country2SmoothedSeries);
            }

            IncidenceChartPlotModel = plotModel;
        }

        #endregion
    }
}

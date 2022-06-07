using System;
using SQLite;

namespace CovidAnalysis.Models.LogEntryItem
{
    [Table(Constants.COVID_ENTRIES_TABLE_NAME)]
    public class LogEntryItemModel : IEntityBase
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }

        // [0] | iso_code
        // ISO 3166-1 alpha-3 – three-letter country codes
        public string IsoCode { get; set; }

        // [2] | location
        // Geographical location
        public string Country { get; set; }

        // [3] | date
        // Date of observation
        public DateTime Date { get; set; }

        // [4] | total_cases
        // Total confirmed cases of COVID-19
        public double TotalCasesOfSickness { get; set; }

        // [10] | total_cases_per_million
        // Total confirmed cases of COVID-19 per 1,000,000 people. Counts can include probable cases, where reported.
        public double TotalCasesOfSicknessPerMillion { get; set; }

        // [5] | new_cases
        // New confirmed cases of COVID-19
        public double NewCasesOfSickness { get; set; }

        // [11] | new_cases_per_million
        // New confirmed cases of COVID-19 per 1,000,000 people. Counts can include probable cases, where reported.
        public double NewCasesOfSicknessPerMillion { get; set; }

        // [7] | total_deaths
        // Total deaths attributed to COVID-19. Counts can include probable deaths, where reported.
        public double TotalDeaths { get; set; }

        // [13] | total_deaths_per_million
        // Total deaths attributed to COVID-19 per 1,000,000 people. Counts can include probable deaths, where reported.
        public double TotalDeathsPerMillion { get; set; }

        // [8] | new_deaths
        // New deaths attributed to COVID-19
        public double NewDeaths { get; set; }

        // [14] | new_deaths_per_million
        /// New deaths attributed to COVID-19 per 1,000,000 people
        public double NewDeathsPerMillion { get; set; }

        // [35] | people_vaccinated
        // Total number of people who received at least one vaccine dose
        public double PeopleVaccinated { get; set; }

        // [41] | people_vaccinated_per_hundred
        // Total number of people who received at least one vaccine dose per 100 people in the total population
        public double PeopleVaccinatedPerHundred { get; set; }

        // [46] | new_people_vaccinated_smoothed_per_hundred
        // Daily number of people receiving their first vaccine dose (7-day smoothed) per 100 people in the total population
        public double NewPeopleVaccinatedSmoothedPerHundred { get; set; }

        public double GetPropertyValueByName(string propertyName)
        {
            return (double)GetType().GetProperty(propertyName).GetValue(this);
        }

        public void SetPropertyValueByName(string propertyName, double value)
        {
            GetType().GetProperty(propertyName).SetValue(this, value);
        }
    }
}

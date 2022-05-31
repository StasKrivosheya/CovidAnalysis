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
        public float TotalCasesOfSickness { get; set; }

        // [10] | total_cases_per_million
        // Total confirmed cases of COVID-19 per 1,000,000 people. Counts can include probable cases, where reported.
        public float TotalCasesOfSicknessPerMillion { get; set; }

        // [5] | new_cases
        // New confirmed cases of COVID-19
        public float NewCasesOfSickness { get; set; }

        // [11] | new_cases_per_million
        // New confirmed cases of COVID-19 per 1,000,000 people. Counts can include probable cases, where reported.
        public float NewCasesOfSicknessPerMillion { get; set; }

        // [7] | total_deaths
        // Total deaths attributed to COVID-19. Counts can include probable deaths, where reported.
        public float TotalDeaths { get; set; }

        // [13] | total_deaths_per_million
        // Total deaths attributed to COVID-19 per 1,000,000 people. Counts can include probable deaths, where reported.
        public float TotalDeathsPerMillion { get; set; }

        // [8] | new_deaths
        // New deaths attributed to COVID-19
        public float NewDeaths { get; set; }

        // [14] | new_deaths_per_million
        /// New deaths attributed to COVID-19 per 1,000,000 people
        public float NewDeathsPerMillion { get; set; }

        // [35] | people_vaccinated
        // Total number of people who received at least one vaccine dose
        public float PeopleVaccinated { get; set; }

        // [41] | people_vaccinated_per_hundred
        // Total number of people who received at least one vaccine dose per 100 people in the total population
        public float PeopleVaccinatedPerHundred { get; set; }

        // [46] | new_people_vaccinated_smoothed_per_hundred
        // Daily number of people receiving their first vaccine dose (7-day smoothed) per 100 people in the total population
        public float NewPeopleVaccinatedSmoothedPerHundred { get; set; }
    }
}

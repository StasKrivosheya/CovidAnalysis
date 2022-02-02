using System;

namespace CovidAnalysis.Models.LogEntryItem
{
    public class LogEntryItemModel
    {
        // [0]
        public string IsoCode;

        // [2] | location
        public string Country;

        // [3]
        public DateTime Date;

        // [4] | total_cases
        // Total confirmed cases of COVID-19
        public int CurrentlySick;

        // [5] | new_cases
        // New confirmed cases of COVID-19
        public int NewCasesOfSickness;

        // [12]
        // New confirmed cases of COVID-19 (7-day smoothed) per 1,000,000 people
        public double NewCasesSmoothedPerMillion;

        // [7]
        public int TotalDeaths;

        // [8] | new_deaths
        // New deaths attributed to COVID-19
        public int NewDeathsForToday;

        // [15]
        // New deaths attributed to COVID-19 (7-day smoothed) per 1,000,000 people
        public double NewDeathsSmoothedPerMillion;
    }
}

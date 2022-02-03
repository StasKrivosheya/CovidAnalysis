using System;
using SQLite;

namespace CovidAnalysis.Models.LogEntryItem
{
    [Table(Constants.COVID_ENTRIES_TABLE_NAME)]
    public class LogEntryItemModel : IEntityBase
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }

        // [0]
        public string IsoCode { get; set; }

        // [2] | location
        public string Country { get; set; }

        // [3]
        public DateTime Date { get; set; }

        // [4] | total_cases
        // Total confirmed cases of COVID-19
        public int CurrentlySick { get; set; }

        // [5] | new_cases
        // New confirmed cases of COVID-19
        public int NewCasesOfSickness { get; set; }

        // [12]
        // New confirmed cases of COVID-19 (7-day smoothed) per 1,000,000 people
        public double NewCasesSmoothedPerMillion { get; set; }

        // [7]
        public int TotalDeaths { get; set; }

        // [8] | new_deaths
        // New deaths attributed to COVID-19
        public int NewDeathsForToday { get; set; }

        // [15]
        // New deaths attributed to COVID-19 (7-day smoothed) per 1,000,000 people
        public double NewDeathsSmoothedPerMillion { get; set; }
    }
}

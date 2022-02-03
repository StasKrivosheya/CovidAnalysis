using System;
using System.Globalization;
using System.Linq;
using CovidAnalysis.Models.LogEntryItem;

namespace CovidAnalysis.Extensions
{
    public static class StringExtension
    {
        public static LogEntryItemModel ToLogEntryItemModel(this string line)
        {
            var values = line.Split(',');

            if (values.Count() != Constants.CSV_DATA_COLUMNS_AMOUNT)
            {
                return null;
            }

            var yyyyMmDd = values[3].Split('-');
            var entryDate = new DateTime(year: int.Parse(yyyyMmDd[0]), month: int.Parse(yyyyMmDd[1]), day: int.Parse(yyyyMmDd[2]));

            if (!double.TryParse(values[4], NumberStyles.Any, CultureInfo.InvariantCulture, out var currentlySick))
            {
                currentlySick = 0;
            }

            if (!double.TryParse(values[5], NumberStyles.Any, CultureInfo.InvariantCulture, out var newCasesOfSickness))
            {
                newCasesOfSickness = 0;
            }

            if (!double.TryParse(values[7], NumberStyles.Any, CultureInfo.InvariantCulture, out var totalDeaths))
            {
                totalDeaths = 0;
            }

            if (!double.TryParse(values[8], NumberStyles.Any, CultureInfo.InvariantCulture, out var newDeathsForToday))
            {
                newDeathsForToday = 0;
            }

            if (!double.TryParse(values[12], NumberStyles.Any, CultureInfo.InvariantCulture, out var newCasesSmoothedPerMillion))
            {
                newCasesSmoothedPerMillion = 0d;
            }

            if (!double.TryParse(values[15], NumberStyles.Any, CultureInfo.InvariantCulture, out var newDeathsSmoothedPerMillion))
            {
                newDeathsSmoothedPerMillion = 0d;
            }

            var parsedEndtry = new LogEntryItemModel
            {
                IsoCode = values[0],
                Country = values[2],
                Date = entryDate,
                CurrentlySick = (int)currentlySick,
                NewCasesOfSickness = (int)newCasesOfSickness,
                NewCasesSmoothedPerMillion = newCasesSmoothedPerMillion,
                TotalDeaths = (int)totalDeaths,
                NewDeathsForToday = (int)newDeathsForToday,
                NewDeathsSmoothedPerMillion = newDeathsSmoothedPerMillion,
            };

            return parsedEndtry;
        }
    }
}

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

            if (!float.TryParse(values[4], NumberStyles.Any, CultureInfo.InvariantCulture, out var totalCasesOfSickness))
            {
                totalCasesOfSickness = 0;
            }

            if (!float.TryParse(values[10], NumberStyles.Any, CultureInfo.InvariantCulture, out var totalCasesOfSicknessPerMillion))
            {
                totalCasesOfSicknessPerMillion = 0;
            }

            if (!float.TryParse(values[5], NumberStyles.Any, CultureInfo.InvariantCulture, out var newCasesOfSickness))
            {
                newCasesOfSickness = 0;
            }

            if (!float.TryParse(values[11], NumberStyles.Any, CultureInfo.InvariantCulture, out var newCasesOfSicknessPerMillion))
            {
                newCasesOfSicknessPerMillion = 0;
            }

            if (!float.TryParse(values[7], NumberStyles.Any, CultureInfo.InvariantCulture, out var totalDeaths))
            {
                totalDeaths = 0;
            }

            if (!float.TryParse(values[13], NumberStyles.Any, CultureInfo.InvariantCulture, out var totalDeathsPerMillion))
            {
                totalDeathsPerMillion = 0;
            }

            if (!float.TryParse(values[8], NumberStyles.Any, CultureInfo.InvariantCulture, out var newDeaths))
            {
                newDeaths = 0;
            }

            if (!float.TryParse(values[14], NumberStyles.Any, CultureInfo.InvariantCulture, out var newDeathsPerMillion))
            {
                newDeathsPerMillion = 0;
            }

            if (!float.TryParse(values[35], NumberStyles.Any, CultureInfo.InvariantCulture, out var peopleVaccinated))
            {
                peopleVaccinated = 0;
            }

            if (!float.TryParse(values[41], NumberStyles.Any, CultureInfo.InvariantCulture, out var peopleVaccinatedPerHundred))
            {
                peopleVaccinatedPerHundred = 0;
            }

            if (!float.TryParse(values[46], NumberStyles.Any, CultureInfo.InvariantCulture, out var newPeopleVaccinatedSmoothedPerHundred))
            {
                newPeopleVaccinatedSmoothedPerHundred = 0;
            }

            var parsedEndtry = new LogEntryItemModel
            {
                IsoCode = values[0],
                Country = values[2],
                Date = entryDate,
                TotalCasesOfSickness = totalCasesOfSickness,
                TotalCasesOfSicknessPerMillion = totalCasesOfSicknessPerMillion,
                NewCasesOfSickness = newCasesOfSickness,
                NewCasesOfSicknessPerMillion = newCasesOfSicknessPerMillion,
                TotalDeaths = totalDeaths,
                TotalDeathsPerMillion = totalDeathsPerMillion,
                NewDeaths = newDeaths,
                NewDeathsPerMillion = newDeathsPerMillion,
                PeopleVaccinated = peopleVaccinated,
                PeopleVaccinatedPerHundred = peopleVaccinatedPerHundred,
                NewPeopleVaccinatedSmoothedPerHundred = newPeopleVaccinatedSmoothedPerHundred,
            };

            return parsedEndtry;
        }
    }
}

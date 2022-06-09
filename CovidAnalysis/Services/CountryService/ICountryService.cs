using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CovidAnalysis.Models.CountryItem;

namespace CovidAnalysis.Services.CountryService
{
    public interface ICountryService
    {
        Task<List<CountryItemModel>> GetCountriesListAsync();

        Task<CountryItemModel> GetCountryAsync(Expression<Func<CountryItemModel, bool>> predicate);

        Task<int> InsertEntriesAsync(IEnumerable<CountryItemModel> entries);

        Task<int> DeleteAllCountriesAsync();
    }
}

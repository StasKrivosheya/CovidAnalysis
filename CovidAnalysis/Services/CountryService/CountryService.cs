using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CovidAnalysis.Models.CountryItem;
using CovidAnalysis.Services.Repository;

namespace CovidAnalysis.Services.CountryService
{
    public class CountryService : ICountryService
    {
        private readonly IRepository _repository;

        public CountryService(IRepository repository)
        {
            _repository = repository;

            _repository.CreateTableAsync<CountryItemModel>();
        }

        public Task<List<CountryItemModel>> GetCountriesListAsync()
        {
            return _repository.GetItemsAsync<CountryItemModel>();
        }

        public Task<CountryItemModel> GetCountryAsync(Expression<Func<CountryItemModel, bool>> predicate)
        {
            return _repository.GetItemAsync(predicate); 
        }

        public Task<int> InsertEntriesAsync(IEnumerable<CountryItemModel> entries)
        {
            return _repository.InsertItemsRangeAsync(entries);
        }

        public Task<int> DeleteAllCountriesAsync()
        {
            return _repository.DeleteAllAsync<CountryItemModel>();
        }
    }
}

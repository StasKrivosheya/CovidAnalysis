using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CovidAnalysis.Models.LogEntryItem;
using CovidAnalysis.Services.Repository;

namespace CovidAnalysis.Services.LogEntryService
{
    public class LogEntryService : ILogEntryService
    {
        private readonly IRepository _repository;

        public LogEntryService(IRepository repository)
        {
            _repository = repository;

            _repository.CreateTableAsync<LogEntryItemModel>();
        }

        #region -- ILogEntryService implementation --

        public Task<List<LogEntryItemModel>> GetEntriesListAsync()
        {
            return _repository.GetItemsAsync<LogEntryItemModel>();
        }

        public Task<List<LogEntryItemModel>> GetEntriesListAsync(Expression<Func<LogEntryItemModel, bool>> predicate)
        {
            return _repository.GetItemsAsync(predicate);
        }

        public Task<LogEntryItemModel> GetEntryAsync(int id)
        {
            return _repository.GetItemAsync<LogEntryItemModel>(id);
        }

        public Task<LogEntryItemModel> GetEntryAsync(Expression<Func<LogEntryItemModel, bool>> predicate)
        {
            return _repository.GetItemAsync(predicate);
        }

        public Task<int> InsertEntryAsync(LogEntryItemModel entry)
        {
            return _repository.InsertItemAsync(entry);
        }

        public Task<int> InsertEntriesAsync(IEnumerable<LogEntryItemModel> entries)
        {
            return _repository.InsertItemsRangeAsync(entries);
        }

        public Task<int> UpdateEntriesAsync(LogEntryItemModel entry)
        {
            return _repository.UpdateItemAsync(entry);
        }

        public Task<int> DeleteEntriesAsync(LogEntryItemModel entry)
        {
            return _repository.DeleteItemAsync(entry);
        }

        #endregion
    }
}

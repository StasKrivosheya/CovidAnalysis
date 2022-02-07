using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CovidAnalysis.Models.LogEntryItem;

namespace CovidAnalysis.Services.LogEntryService
{
    public interface ILogEntryService
    {
        Task<List<LogEntryItemModel>> GetEntriesListAsync();
        Task<List<LogEntryItemModel>> GetEntriesListAsync(Expression<Func<LogEntryItemModel, bool>> predicate);

        Task<LogEntryItemModel> GetEntryAsync(int id);
        Task<LogEntryItemModel> GetEntryAsync(Expression<Func<LogEntryItemModel, bool>> predicate);

        Task<int> InsertEntryAsync(LogEntryItemModel entry);
        Task<int> InsertEntriesAsync(IEnumerable<LogEntryItemModel> entries);

        Task<int> UpdateEntriesAsync(LogEntryItemModel entry);

        Task<int> DeleteEntryAsync(LogEntryItemModel entry);
        Task<int> DeleteAllEnrtiesAsync();
    }
}

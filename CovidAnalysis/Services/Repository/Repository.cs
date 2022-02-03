using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CovidAnalysis.Models;
using SQLite;

namespace CovidAnalysis.Services.Repository
{
    public class Repository : IRepository
    {
        private readonly SQLiteAsyncConnection _database;

        public Repository()
        {
            var databasePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            _database = new SQLiteAsyncConnection(Path.Combine(databasePath, Constants.DATABASE_NAME));
        }

        #region --- IRepository implementation ---

        public Task CreateTableAsync<T>() where T : IEntityBase, new()
        {
            return _database.CreateTableAsync<T>();
        }

        public Task<List<T>> GetItemsAsync<T>() where T : IEntityBase, new()
        {
            return _database.Table<T>().ToListAsync();
        }

        public Task<List<T>> GetItemsAsync<T>(Expression<Func<T, bool>> predicate) where T : IEntityBase, new()
        {
            return _database.Table<T>().Where(predicate).ToListAsync();
        }

        public Task<T> GetItemAsync<T>(int id) where T : IEntityBase, new()
        {
            return _database.GetAsync<T>(id);
        }

        public Task<T> GetItemAsync<T>(Expression<Func<T, bool>> predicate) where T : IEntityBase, new()
        {
            return _database.FindAsync(predicate);
        }

        public async Task<int> InsertItemAsync<T>(T item) where T : IEntityBase, new()
        {
            int result;

            try
            {
                result = await _database.InsertAsync(item);
            }
            catch (SQLiteException)
            {
                result = -1;
            }

            return result;
        }

        public async Task<int> InsertItemsRangeAsync<T>(IEnumerable<T> items) where T : IEntityBase, new()
        {
            int result;

            try
            {
                result = await _database.InsertAllAsync(items).ConfigureAwait(false);
            }
            catch (SQLiteException)
            {
                result = -1;
            }

            return result;
        }

        public Task<int> UpdateItemAsync<T>(T item) where T : IEntityBase, new()
        {
            return _database.UpdateAsync(item);
        }

        public Task<int> DeleteItemAsync<T>(T item) where T : IEntityBase, new()
        {
            return _database.DeleteAsync<T>(item.Id);
        }

        #endregion
    }
}

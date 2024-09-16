using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vendor_App.Models;
using SQLite;

namespace Vendor_App.Repositories
{
    public class SQLiteTransactionRepository : ITransactionRepository
    {
        private readonly SQLiteAsyncConnection _database;

        // Constructor
        public SQLiteTransactionRepository(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<Transaction>().Wait(); // Change Transactions to Transaction
        }

        // Save a transaction to the database   
        public Task SaveTransactionAsync(Transaction transaction) // Change Transactions to Transaction
        {
            if (transaction.Id != 0) // If the transaction has an ID, it already exists in the database
            {
                return _database.UpdateAsync(transaction); // Update the existing transaction
            }
            else
            {
                return _database.InsertAsync(transaction); // Insert a new transaction
            }
        }

        // Get all transactions from the database
        public Task<List<Transaction>> GetTransactionsAsync() // Change Transactions to Transaction
        {
            return _database.Table<Transaction>().ToListAsync(); // Get all transactions from the database
        }

        // Get all transactions for a specific date
        public Task<List<Transaction>> GetTransactionsByDateAsync(DateTime date) // Fix method signature to match interface
        {
            return _database.Table<Transaction>()
                            .Where(t => t.Date.Date == date.Date) // Ensure comparison by date only
                            .ToListAsync(); // Get all transactions for a specific date
        }

        // Get the total amount of all transactions
        public async Task<double> GetTotalAmountAsync()
        {
            var transactions = await _database.Table<Transaction>().ToListAsync();
            return transactions.Sum(t => t.Amount);
        }
    }
}

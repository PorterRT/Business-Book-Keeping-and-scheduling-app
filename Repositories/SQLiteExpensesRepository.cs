using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using SQLite;
using Vendor_App.Models;

namespace Vendor_App.Repositories
{
    internal class SQLiteExpensesRepository : IExpensesRepository
    {
        private readonly SQLiteAsyncConnection _database;
        // Constructor
        public SQLiteExpensesRepository(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<Expenses>().Wait(); // Change Transactions to Transaction
        }

        public Task SaveExpenseAsync(Expenses expense) // Save an expense to the database 
        {
            if (expense.ExpenseId != 0) // If the expense has an ID, it already exists in the database
            {
                return _database.UpdateAsync(expense); // Update the existing expense
            }
            else
            {
                return _database.InsertAsync(expense); // Insert a new expense
            }
        }

        public Task UpdateExpenseAsync(Expenses expense)
        {
            return _database.UpdateAsync(expense); // Update the expense in the database
        }

        public Task DeleteExpenseAsync(Expenses expense)
        {
           return _database.DeleteAsync(expense); // Delete the transaction from the database

        }
        public Task<List<Expenses>> GetExpensesForEventAsync(int vendorEventId) // Get all expenses for a specific event
        {
            return _database.Table<Expenses>()
                .Where(e => e.VendorEventId == vendorEventId)
                .ToListAsync();
        }




    }
}

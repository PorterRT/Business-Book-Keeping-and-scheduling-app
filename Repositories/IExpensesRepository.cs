using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vendor_App.Models;
namespace Vendor_App.Repositories
{
    internal interface IExpensesRepository
    {
        public Task SaveExpenseAsync(Expenses expense); // Save an expense to the database

        public Task UpdateExpenseAsync(Expenses expense);

        public Task DeleteExpenseAsync(Expenses expense);
        public Task<List<Expenses>> GetExpensesForEventAsync(int vendorEventId); // Get all expenses for a specific event

    }
}

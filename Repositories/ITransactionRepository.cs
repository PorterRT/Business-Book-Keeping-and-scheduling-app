using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vendor_App.Models;

namespace Vendor_App.Repositories
{
    public interface ITransactionRepository
    {
        Task SaveTransactionAsync(Transaction transaction); // Use singular 'Transaction'
        Task<List<Transaction>> GetTransactionsAsync(); // Use singular 'Transaction'
        Task<List<Transaction>> GetTransactionsByDateAsync(DateTime date); // Use singular 'Transaction' and fix method name
        Task<double> GetTotalAmountAsync();  // Get the total amount of all transactions
    }
}

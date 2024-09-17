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
        Task<List<Transaction>> GetTransactionsByDateAsync(DateTime date); // 
        Task<double> GetTotalAmountAsync();  // Get the total amount of all transactions

        // Add a method to delete a transaction
        Task DeleteTransactionAsync(Transaction transaction); // Use singular 'Transaction'
       


    }
}

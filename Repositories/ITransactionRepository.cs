using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vendor_App.Models;

namespace Vendor_App.Repositories
{
    public interface ITransactionRepository
    {
        Task SaveTransactionAsync(Transaction transaction); // Saves the transaction to the database
        Task<List<Transaction>> GetTransactionsAsync(); // Get all transactions from the database
        Task<List<Transaction>> GetTransactionsByDateAsync(DateTime date); // gets all transactions for a specific date
        Task<double> GetTotalAmountAsync();  // Get the total amount of all transactions

        // Add a method to delete a transaction
        Task DeleteTransactionAsync(Transaction transaction); // Delete a transaction from the database
        Task<List<Transaction>> GetTransactionsByVendorEventAsync(VendorEvents vendorEvent);
    }
}

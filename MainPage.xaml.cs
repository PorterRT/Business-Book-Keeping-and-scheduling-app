namespace Vendor_App
{
    using System.Collections.ObjectModel;
    using Vendor_App.Models;
    using Vendor_App.Repositories;
    public partial class MainPage : ContentPage
    {
        private ITransactionRepository _transactionRepository;
        private double total = 0;

        public MainPage()
        {
            InitializeComponent();

            // Set up the local SQLite database path
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "transactions.db3");
            _transactionRepository = new SQLiteTransactionRepository(dbPath);

            // set the default date to today
            TransactionDatePicker.Date = DateTime.Today;

            // Load the transactions from the database
            LoadTransactions();
        }

        private async void LoadTransactions()
        {
            // Get the transactions from the database
            var transactions = await _transactionRepository.GetTransactionsAsync();
            TransactionList.ItemsSource = transactions;

            // calculate and display the total amount
            await UpdateTotalAmount();
        }
        private async void OnAddTransactionClicked(object sender, EventArgs e)
        {
           // add a try catch and throw an error but then allow the user to keep going if they did not select car do cash
            if (double.TryParse(AmountEntry.Text, out double amount))
            {
                var transaction = new Transaction
                { paymentType = PaymentTypePicker.SelectedItem.ToString() ?? "Unknown", 
                    Amount = amount, 
                    Date = TransactionDatePicker.Date 
                };

                // Save the transaction to the database
                await _transactionRepository.SaveTransactionAsync(transaction);

                // Reload the transactions to refresh the list and total    
                LoadTransactions();




                // Clear the amount entry, payment type picker, and date picker
                AmountEntry.Text = string.Empty;
                PaymentTypePicker.SelectedIndex = -1; // Set the selected index to -1 to clear the selection
                TransactionDatePicker.Date = DateTime.Today;// reset the date to today
                
            }
            else
            {
                // Display an alert if the amount is invalid
                await DisplayAlert("Invalid Amount", "Please enter a valid amount", "OK");
            }
        }
        private async Task UpdateTotalAmount()
        {
            // Get the total amount of all transactions
            total = await _transactionRepository.GetTotalAmountAsync();
            TotalAmountLabel.Text = $"Total Amount: {total:C}";
        }
    }
 }


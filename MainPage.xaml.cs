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
            LoadTransactionsForDate(TransactionDatePicker.Date);
        }

        private async void LoadTransactionsForDate(DateTime selectedDate)
        {
            // Get the transactions from the database and only display 
            var transactions = await _transactionRepository.GetTransactionsByDateAsync(selectedDate);
            TransactionList.ItemsSource = transactions;

            // calculate and display the total amount
            await UpdateTotalDateAmount(selectedDate);
        }
        
        private void OnDateSelected(object sender, DateChangedEventArgs e) 
        {
            // Load the transactions for the selected date
            LoadTransactionsForDate(e.NewDate); 
        }
        private async void OnAddTransactionClicked(object sender, EventArgs e)
        {
            if (double.TryParse(AmountEntry.Text, out double amount))
            {
                try 
                {
                    if (PaymentTypePicker.SelectedItem == null) // error for no payment type selected
                    {
                        throw new Exception("Please select a payment type");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", ex.Message, "OK");
                }
                finally
                {
                    var transaction = new Transaction
                    {
                        paymentType = PaymentTypePicker.SelectedItem.ToString() ?? "Unknown",
                        Amount = amount,
                        Date = TransactionDatePicker.Date
                    };
                    // Save the transaction to the database
                    await _transactionRepository.SaveTransactionAsync(transaction);
                    // Reload the transactions to refresh the list and total    
                    LoadTransactionsForDate(TransactionDatePicker.Date);
                    // Clear the amount entry, payment type picker, and date picker
                    AmountEntry.Text = string.Empty;
                    PaymentTypePicker.SelectedIndex = -1; // Set the selected index to -1 to clear the selection
                }
                // Clear the amount entry, payment type picker, and date picker
                AmountEntry.Text = string.Empty;
                PaymentTypePicker.SelectedIndex = -1; // Set the selected index to -1 to clear the selection
                
            }
            else
            {
                // Display an alert if the amount is invalid
                await DisplayAlert("Invalid Amount", "Please enter a valid amount", "OK");
            }
        }
        // Delete a transaction
        private async void OnDeleteSwipeInvoked(object sender, EventArgs e)
        {
            var DeleteSlider = (SwipeItem)sender;
            var transaction = (Transaction)DeleteSlider.CommandParameter;

            bool confirm = await DisplayAlert("Confirm", "Are you sure you want to delete this transaction?", "Yes", "No");
            if (confirm)
            {
                // Delete the transaction from the database
                await _transactionRepository.DeleteTransactionAsync(transaction);
                // Reload the transactions to refresh the list and total
                LoadTransactionsForDate(TransactionDatePicker.Date);
                

            }
        }
        private async Task UpdateTotalDateAmount(DateTime selectedDate)
        {
            // Get the total amount for the selected date
            var transactions = await _transactionRepository.GetTransactionsByDateAsync(selectedDate);
            total = transactions.Sum(t => t.Amount);
            TotalAmountLabel.Text = $"Total: {total:C}";
        }
    }
 }


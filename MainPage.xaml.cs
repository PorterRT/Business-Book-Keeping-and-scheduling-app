namespace Vendor_App
{
    using System.Collections.ObjectModel;
    using Vendor_App.Models;
    using Vendor_App.Repositories;
    public partial class MainPage : ContentPage
    {
        private ITransactionRepository _transactionRepository;
        private IVendorEventRepository _vendorEventRepository;
        public ObservableCollection<VendorEvents> Events { get; set; }

        private double total = 0;

        public MainPage()
        {
            InitializeComponent();

            // Set up the local SQLite database path
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "transactions.db3");
            _transactionRepository = new SQLiteTransactionRepository(dbPath);

            string eventsDbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "vendorEvents.db3");
            _vendorEventRepository = new SQLiteVendorEventRepository(eventsDbPath);

            // Initialize the ObservableCollection to hold the events
            Events = new ObservableCollection<VendorEvents>();
            EventsListView.ItemsSource = Events; // Bind the ListView to the ObservableCollection


            BindingContext = this;

            // set the default date to today
            TransactionDatePicker.Date = DateTime.Today;

            // Load the transactions from the database
            
            LoadTransactionsForDate(TransactionDatePicker.Date);
            LoadVendorEventsByDate(TransactionDatePicker.Date);

        }

        private async void LoadTransactionsForDate(DateTime selectedDate)
        {
            // Get the transactions from the database and only display 
            var transactions = await _transactionRepository.GetTransactionsByDateAsync(selectedDate);
            TransactionList.ItemsSource = new ObservableCollection<Transaction>(transactions);

            // calculate and display the total amount
            await UpdateTotalDateAmount(selectedDate);
        }

        private async void LoadTransactionsForVendorEvent(VendorEvents vendorEvent)
        {
            // Get the transactions from the database and only display 
            var transactions = await _transactionRepository.GetTransactionsByVendorEventAsync(vendorEvent);
            TransactionList.ItemsSource = new ObservableCollection<Transaction>(transactions);

            // calculate and display the total amount
            await UpdateTotalDateAmount(vendorEvent.EventDate);
        }
        private async Task LoadVendorEventsByDate(DateTime selectedDate)
        {
            try
            {
                // Clear the previous events
                Events.Clear();

                // Fetch all events using the repository
                var allEvents = await _vendorEventRepository.GetVendorEventsByDateAsync(TransactionDatePicker.Date);

                // Add each fetched event to the ObservableCollection
                foreach (var vendorEvent in allEvents)
                {
                    Events.Add(vendorEvent);
                }

                // Log event details for debugging
                Console.WriteLine($"Loaded {allEvents.Count()} events.");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load events: {ex.Message}", "OK");
            }
        }







        private void OnDateSelected(object sender, DateChangedEventArgs e) 
        {
            // Load the transactions for the selected date
            LoadTransactionsForDate(e.NewDate);
            LoadVendorEventsByDate(e.NewDate);
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

        // Event handler for the button click to navigate to VendorEventManager
        private async void OnNavigateToVendorEventManagerClicked(object sender, EventArgs e)
        {
            try
            {
                // Attempt to navigate to VendorEventManager
                await Navigation.PushAsync(new VendorEventManager());
            }
            catch (Exception ex)
            {
                // Display the exception message for debugging
                await DisplayAlert("Navigation Error", ex.Message, "OK");
                Console.WriteLine($"Navigation Error: {ex}");
            }
        }

        private async void OnNavigateToOrderEventViewerClicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new VendorEventViewer());
            }
            catch (Exception ex)
            {
                // Display the exception message for debugging
                await DisplayAlert("Navigation Error", ex.Message, "OK");
                Console.WriteLine($"Navigation Error: {ex}");
            }
        }

    }
}


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

            // Initialize the ObservableCollection for events
            Events = new ObservableCollection<VendorEvents>();
            VendorEventPicker.ItemsSource = Events; // Bind the Picker to the ObservableCollection

            BindingContext = this;

            // Set the default date to today
            TransactionDatePicker.Date = DateTime.Today;

            // Load the events and transactions
            LoadVendorEventsByDate(TransactionDatePicker.Date);
            VendorEventPicker.SelectedIndexChanged += OnVendorEventSelected;
        }

        private async Task LoadVendorEventsByDate(DateTime selectedDate)
        {
            try
            {
                // Clear the previous events
                Events.Clear();

                // Fetch all events using the repository
                var allEvents = await _vendorEventRepository.GetVendorEventsByDateAsync(selectedDate);

                // Add each fetched event to the ObservableCollection
                foreach (var vendorEvent in allEvents)
                {
                    Events.Add(vendorEvent);
                }

                // Log event details for debugging
                Console.WriteLine($"Loaded {allEvents.Count()} events.");

                // Automatically select the first event, if any
                if (Events.Count > 0)
                {
                    VendorEventPicker.SelectedItem = Events.First();
                    LoadTransactionsForVendorEvent((VendorEvents)VendorEventPicker.SelectedItem);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load events: {ex.Message}", "OK");
            }
        }

        private async void LoadTransactionsForVendorEvent(VendorEvents vendorEvent)
        {
            if (vendorEvent == null)
            {
                await DisplayAlert("Error", "Please select a valid vendor event", "OK");
                return;
            }

            // Get transactions for the selected vendor event
            var transactions = await _transactionRepository.GetTransactionsByVendorEventAsync(vendorEvent.VendorEventId);

            // Bind the transactions to the UI (e.g., a ListView or CollectionView)
            TransactionList.ItemsSource = new ObservableCollection<Transaction>(transactions);

            // Optionally, update the total amount for the selected event
            await UpdateTotalEventAmount(vendorEvent);
        }

        private async Task UpdateTotalEventAmount(VendorEvents vendorEvent)
        {
            var transactions = await _transactionRepository.GetTransactionsByVendorEventAsync(vendorEvent.VendorEventId);
            total = transactions.Sum(t => t.Amount);
            TotalAmountLabel.Text = $"Total: {total:C}";
        }

        private async void OnVendorEventSelected(object sender, EventArgs e)
        {
            var selectedEvent = (VendorEvents)VendorEventPicker.SelectedItem;
            if (selectedEvent != null)
            {
                LoadTransactionsForVendorEvent(selectedEvent); // Reload the transactions for the selected event
            }
        }

        private async void OnAddTransactionClicked(object sender, EventArgs e)
        {
            if (double.TryParse(AmountEntry.Text, out double amount))
            {
                try
                {
                    if (PaymentTypePicker.SelectedItem == null)
                    {
                        throw new Exception("Please select a payment type");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", ex.Message, "OK");
                    return;
                }

                // Get the selected event from the picker
                var selectedEvent = (VendorEvents)VendorEventPicker.SelectedItem;
                if (selectedEvent == null)
                {
                    await DisplayAlert("Error", "Please select a vendor event", "OK");
                    return;
                }

                // Create a new transaction, linking it to the selected event's VendorEventId
                var transaction = new Transaction
                {
                    paymentType = PaymentTypePicker.SelectedItem.ToString(),
                    Amount = amount,
                    Date = TransactionDatePicker.Date,
                    VendorEventId = selectedEvent.VendorEventId // Foreign key reference to the selected event
                };

                // Save the transaction to the database
                await _transactionRepository.SaveTransactionAsync(transaction);

                // Reload the transactions to refresh the list and total
                LoadTransactionsForVendorEvent(selectedEvent);

                // Clear inputs
                AmountEntry.Text = string.Empty;
                PaymentTypePicker.SelectedIndex = -1;
            }
            else
            {
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

        private async void OnDateSelected(object sender, DateChangedEventArgs e)
        {
            // Load vendor events for the selected date
            await LoadVendorEventsByDate(e.NewDate);
        }

        private async void LoadTransactionsForDate(DateTime selectedDate)
        {
            // Get the transactions from the database and only display 
            var transactions = await _transactionRepository.GetTransactionsByDateAsync(selectedDate);
            TransactionList.ItemsSource = new ObservableCollection<Transaction>(transactions);

            // calculate and display the total amount
            await UpdateTotalDateAmount(selectedDate);
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

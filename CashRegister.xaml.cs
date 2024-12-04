namespace Vendor_App
{
    using System.Collections.ObjectModel;
    using Vendor_App.Models;
    using Vendor_App.Repositories;

    public partial class CashRegister : ContentPage
    {
        private ITransactionRepository _transactionRepository;
        private IVendorEventRepository _vendorEventRepository;
        public ObservableCollection<VendorEvents> Events { get; set; }

        private double total = 0;
        private double processingFee = 0;

        public CashRegister()
        {
            InitializeComponent();

            var databaseConnection = new DatabaseConnection();

            // Initialize repositories
            _transactionRepository = databaseConnection.VendorDatabaseConnection();
            _vendorEventRepository = databaseConnection.EventDatabaseConnection();

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
        // This Loads transactions for the selected Event
        private async void OnVendorEventSelected(object sender, EventArgs e)
        {
            var selectedEvent = (VendorEvents)VendorEventPicker.SelectedItem;
            if (selectedEvent != null)
            {
                LoadTransactionsForVendorEvent(selectedEvent); // Reload the transactions for the selected event
            }
        }

        public double CalulateProcessingFees(double Amount, string PaymentType)
        {
            if (PaymentType == "Square")
            {
                processingFee = (Amount * 0.026) + 0.10;
            }
            else if (PaymentType == "Venmo")
            {
                processingFee = (Amount * 0.019) + 0.10;
            }
            else if (PaymentType == "Cash App")
            {
                processingFee = Amount * 0.0275;
            }
            else if (PaymentType == "Apple Pay")
            {
                processingFee = 0.00;
            }
            else if (PaymentType == "Credit Card")
            {
                processingFee = (Amount * 0.03); // this is a place holder may need to get each card companies fee
            }
            else if (PaymentType == "Cash")
            {
                processingFee = 0.00;
            }
            return processingFee;
        }

        // Event handler for the button click to add a transaction which uploads the transaction to the database
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
                    ProcessingFee = CalulateProcessingFees(amount, PaymentTypePicker.SelectedItem.ToString()),
                    Date = TransactionDatePicker.Date,
                    Time = DateTime.Now,
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

        // update transaction
        private async void OnUpdateSwipeInvoked(object sender, EventArgs e)
        {
            var swipeItem = (SwipeItem)sender;
            var transaction = (Transaction)swipeItem.CommandParameter;

            // Display the current amount in a prompt for the user to update
            string newAmount = await DisplayPromptAsync("Update Transaction",
                                                        "Enter new amount:",
                                                        initialValue: transaction.Amount.ToString(),
                                                        keyboard: Keyboard.Numeric);
            // Get the original payment type
            string originalPaymentType = transaction.paymentType;

            // Construct the list of options, putting the original payment type at the top
            var paymentTypes = new List<string>
            {
                originalPaymentType, // Original payment type first
                "Cash",
                "Credit Card",
                "Square",
                "Venmo",
                "Cash App",
                "Apple Pay"
            };

            // Remove the original from the list to avoid duplication if it exists in the default set
            paymentTypes = paymentTypes.Distinct().ToList();

            // Display the action sheet with the original payment type at the top
            string newPaymentType = await DisplayActionSheet(
                "Select new payment type, the top is the original",
                "Cancel",  // Cancel button
                null,      // No destructive button
                paymentTypes.ToArray()); // Dynamic list of payment types

            if (!string.IsNullOrEmpty(newPaymentType) && newPaymentType != "Cancel")
            {
                try
                {
                    // Update the transaction with new values
                    transaction.Amount = double.Parse(newAmount);
                }
                catch (FormatException exception)
                {
                    // Notify the user about the invalid input
                    await DisplayAlert("Invalid Input", "Please enter a valid numeric amount.", "OK");
                    return; // Exit the method to prevent further processing
                }
                
                // Update the transaction with new values
               // transaction.Amount = double.Parse(newAmount);
                // Update the transaction with the selected payment type
                transaction.paymentType = newPaymentType;
                
                transaction.ProcessingFee = CalulateProcessingFees(double.Parse(newAmount), newPaymentType);

                // Save the updated transaction to the database
                await _transactionRepository.UpdateTransactionAsync(transaction);

                // Refresh the transaction list
                LoadTransactionsForVendorEvent((VendorEvents)VendorEventPicker.SelectedItem);
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
                LoadTransactionsForVendorEvent((VendorEvents)VendorEventPicker.SelectedItem);
            }
        }


        // Event handler for the date picker to load Events for the selected date
        private async void OnDateSelected(object sender, DateChangedEventArgs e)
        {
            // Load vendor events for the selected date
            await LoadVendorEventsByDate(e.NewDate);
        }
        
      
      
    }
}

namespace Vendor_App
{
    using System.Collections.ObjectModel;
    using System.Windows.Input;
    using System.ComponentModel;
    using System.Xml.Linq;
    using Vendor_App.Models;
    using Vendor_App.Repositories;
    using Utillites;
    public partial class CashRegister : ContentPage
    {
        private ITransactionRepository _transactionRepository;
        private IVendorEventRepository _vendorEventRepository;
        private IExpensesRepository _expensesRepository;
        public ICommand RefreshCommand { get; set; }
        private bool _isRefreshing;

        public event PropertyChangedEventHandler PropertyChanged;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                if (_isRefreshing != value)
                {
                    _isRefreshing = value;
                    OnPropertyChanged(nameof(IsRefreshing));
                }
            }
        }
        public ObservableCollection<VendorEvents> Events { get; set; }
        public ObservableCollection<Expenses> ExpenseList { get; set; }

        private double total = 0;
        private double processingFee = 0;
        private double Expenses = 0;
        private bool _isExpenseToggled = false;
        private bool _isFeeToggled = false;

        public CashRegister()
        {
            InitializeComponent();

            var databaseConnection = new DatabaseConnection();

            // Initialize repositories
            _transactionRepository = databaseConnection.VendorDatabaseConnection();
            _vendorEventRepository = databaseConnection.EventDatabaseConnection();
            _expensesRepository = databaseConnection.ExpensesDatabaseConnection();

            // Initialize the ObservableCollection for events
            Events = new ObservableCollection<VendorEvents>();
            VendorEventPicker.ItemsSource = Events; // Bind the Picker to the ObservableCollection

            ExpenseList = new ObservableCollection<Expenses>();
            ExpensesList.ItemsSource = ExpenseList; // Bind the ListView to the ObservableCollection

            RefreshCommand = new Command(async () => await RefreshCommandAsync());
            
            BindingContext = this;

            // Set the default date to today
            TransactionDatePicker.Date = DateTime.Today;

            //default for tax label and switch
            TaxDeductibleLabel.IsVisible = false;
            TaxDeductibleSwitch.IsVisible = false;

            // Set the default Tip amount to 0.00
            // TipAmountEntry.Text = "0.00";

            // Load the events and transactions
            LoadVendorEventsByDate(TransactionDatePicker.Date);
            VendorEventPicker.SelectedIndexChanged += OnVendorEventSelected;
        }
        private async Task RefreshCommandAsync()
        {
            IsRefreshing = true;
            try
            {
                 
                // Refresh the Picker's data (Vendor Events)
                await LoadVendorEventsByDate(TransactionDatePicker.Date);

                // Refresh Transactions and Expenses for the selected event, if any
                 if ((VendorEventPicker.SelectedItem is VendorEvents selectedEvent))
                {
                     await LoadTransactionsForVendorEvent(selectedEvent);
                     await LoadExpensesForVendorEvent(selectedEvent);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Failed to refresh data: " + ex.Message, "OK");
            }
            finally
            {
                IsRefreshing = false; // Ensure refreshing stops
            }
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
                    LoadExpensesForVendorEvent((VendorEvents)VendorEventPicker.SelectedItem);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load events: {ex.Message}", "OK");
            }
        }

        private async Task LoadTransactionsForVendorEvent(VendorEvents vendorEvent)
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

        private async Task LoadExpensesForVendorEvent(VendorEvents vendorEvent)
        {
            if (vendorEvent == null)
            {
                await DisplayAlert("Error", "Please select a valid vendor event", "OK");
                return;
            }
            // Get expenses for the selected vendor event
            var expenses = await _expensesRepository.GetExpensesForEventAsync(vendorEvent.VendorEventId);
            // Bind the expenses to the UI (e.g., a ListView or CollectionView)
            ExpensesList.ItemsSource = new ObservableCollection<Expenses>(expenses);
            // Optionally, update the total amount for the selected event
            await UpdateTotalEventAmount(vendorEvent);
        }   

        private async Task UpdateTotalEventAmount(VendorEvents vendorEvent)
        {
            if (TransactionExpenseSwitch.Source.ToString().Contains("minuscash.png"))
            {
                var expenses = await _expensesRepository.GetExpensesForEventAsync(vendorEvent.VendorEventId);
                Expenses = expenses.Sum(e => e.Amount);
                TotalAmountLabel.Text = $"Total: {Expenses:C}";
            }
            else
            {
                var transactions = await _transactionRepository.GetTransactionsByVendorEventAsync(vendorEvent.VendorEventId);
                total = transactions.Sum(t => t.Amount + t.Tip);
                if (FeeEstimateSwitch.Source.ToString().Contains("colorcredit.png"))
                {
                    total -= transactions.Sum(t => t.ProcessingFee);
                }
                TotalAmountLabel.Text = $"Total: {total:C}";
            }
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

            switch (PaymentType)
            {
                case "Square":
                    {
                        processingFee = (Amount * 0.026) + 0.10;
                        break;
                    }
                case "Venmo":
                    {
                        processingFee = (Amount * 0.019) + 0.10;
                        break;
                    }
                case "Cash App":
                    {
                        processingFee = Amount * 0.0275;
                        break;
                    }
                case "Apple Pay":
                    {
                        processingFee = 0.00;
                        break;
                    }
                case "Credit Card":
                    {
                        processingFee = (Amount * 0.03); // this is a place holder may need to get each card companies fee
                        break;
                    }
                case "Cash":
                    {
                        processingFee = 0.00;
                        break;
                    }
                default:
                    {
                        processingFee = 0.00;
                        break;

                    }
            }
                    return processingFee;
            
            }

        private void OnFeeEstimateToggled(object sender, EventArgs e)
        {
            _isFeeToggled = !_isFeeToggled;
            FeeEstimateSwitch.Source = _isFeeToggled ? "colorcredit.png" : "greycredit.png";
            FeeInTransactionVisible.IsToggled = FeeEstimateSwitch.Source.ToString().Contains("colorcredit.png");
            // Update the total amount when the switch is toggled
            var selectedEvent = (VendorEvents)VendorEventPicker.SelectedItem;
            if (selectedEvent != null)
            {
                UpdateTotalEventAmount(selectedEvent);
            }
            
        }

        private void OnTaxExpenseToggled(object sender, ToggledEventArgs t)
        {
            bool IsTaxExpense = t.Value;

        }

        // Event handler for the button click to add a transaction which uploads the transaction to the database
        private async void OnAddTransactionClicked(object sender, EventArgs e)
        {
            if (!(TransactionExpenseSwitch.Source.ToString().Contains("minuscash.png")))
            {

                if (double.TryParse(AmountEntry.Text, out double amount))
                {
                    if (double.TryParse(TipAmountEntry.Text, out double tipAmount) || string.IsNullOrEmpty(TipAmountEntry.Text))
                    {
                        if (string.IsNullOrEmpty(TipAmountEntry.Text))
                        {
                            tipAmount = 0.00;
                        }
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
                            Tip = tipAmount, // Set the tip amount
                            ProcessingFee = CalulateProcessingFees(amount + tipAmount, 
                                PaymentTypePicker.SelectedItem.ToString()),
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
                        TipAmountEntry.Text = string.Empty; // Reset the tip amount to default
                        PaymentTypePicker.SelectedIndex = -1;

                        // Unfocus the entry fields to hide the keyboard
                        AmountEntry.Unfocus();
                        TipAmountEntry.Unfocus();
                        PaymentTypePicker.Unfocus();
                    }

                    else
                    {
                        await DisplayAlert("Invalid Tip Amount", "Please enter a valid amount", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Invalid Amount", "Please enter a valid amount", "OK");
                }
            }
            else if (TransactionExpenseSwitch.Source.ToString().Contains("minuscash.png"))
            {
                if (double.TryParse(AmountEntry.Text, out double amount))
                {

                    var selectedEvent = (VendorEvents)VendorEventPicker.SelectedItem;
                    if (selectedEvent == null)
                    {
                        await DisplayAlert("Error", "Please select a vendor event", "OK");
                        return;
                    }

                    // Detailed logging
                    Console.WriteLine($"Preparing to save expense:");
                    Console.WriteLine($"Amount: {amount}");
                    Console.WriteLine($"Label: {ExpenseLabelEntry.Text}");
                    Console.WriteLine($"Date: {TransactionDatePicker.Date}");
                    Console.WriteLine($"VendorEventId: {selectedEvent.VendorEventId}");
                    if (ExpenseLabelEntry.Text != null)
                    {
                        var Expense = new Expenses
                        {
                            Amount = amount,
                            Label = ExpenseLabelEntry.Text,
                            Date = TransactionDatePicker.Date,
                            IsTaxDeductible = TaxDeductibleSwitch.IsToggled, // Set the tax-deductible status
                            VendorEventId = selectedEvent.VendorEventId
                        };

                        Console.WriteLine($"Expense object created. About to save to database.");

                        await _expensesRepository.SaveExpenseAsync(Expense);

                        Console.WriteLine($"Expense saved. Now loading expenses for event.");

                        LoadExpensesForVendorEvent(selectedEvent);

                        AmountEntry.Text = string.Empty;
                        ExpenseLabelEntry.Text = string.Empty;
                        TaxDeductibleSwitch.IsToggled = false;

                        AmountEntry.Unfocus();
                        ExpenseLabelEntry.Unfocus();
                    }

                    else
                    {
                        await DisplayAlert("Error", "Please enter a label for the expense", "OK");
                        return;
                    }


                }
            }
        }

        // update transaction
        private async void OnUpdateSwipeInvoked(object sender, EventArgs e)
        {
            if (TransactionExpenseSwitch.Source.ToString().Contains("minuscash.png"))
            {
                var swipeItem = (SwipeItem)sender;
                var expense = (Expenses)swipeItem.CommandParameter;
                // Display the current amount in a prompt for the user to update
                string newAmount = await DisplayPromptAsync("Update Expense",
                                                            "Enter new amount:",
                                                            initialValue: expense.Amount.ToString(),
                                                            keyboard: Keyboard.Numeric);

                // Display the current label in a prompt for the user to update
                string newLabel = await DisplayPromptAsync("Update Expense",
                                                           "Enter new label:",
                                                           initialValue: expense.Label);

                // Display the current tax-deductible status in a prompt for the user to update
                bool newIsTaxDeductible = await DisplayAlert("Update Expense",
                                                             "Is this expense tax-deductible?",
                                                             "Yes", "No");
                try
                {
                    // Update the expense with new values
                    expense.Amount = double.Parse(newAmount);
                }
                catch (FormatException exception)
                {
                    // Notify the user about the invalid input
                    await DisplayAlert("Invalid Input", "Please enter a valid numeric amount.", "OK");
                    return; // Exit the method to prevent further processing
                }
                try
                {
                    expense.Label = newLabel;
                }
                catch (FormatException exception)
                {
                    await DisplayAlert("Invalid Input", "Please enter a valid Labal amount.", "OK");
                    return; // Exit the method to prevent further processing
                }

                try
                {
                    expense.IsTaxDeductible = newIsTaxDeductible;
                }
                catch (FormatException exception)
                {
                    await DisplayAlert("Invalid Input", "Please enter a valid tax deducation value.", "OK");
                    return; // Exit the method to prevent further processing
                }
                // Save the updated expense to the database
                await _expensesRepository.UpdateExpenseAsync(expense);
                // Refresh the expense list
                LoadExpensesForVendorEvent((VendorEvents)VendorEventPicker.SelectedItem);
            }
            else
            {
                var swipeItem = (SwipeItem)sender;
                var transaction = (Transaction)swipeItem.CommandParameter;

                // Display the current amount in a prompt for the user to update
                string newAmount = await DisplayPromptAsync("Update Transaction",
                                                            "Enter new amount:",
                                                            initialValue: transaction.Amount.ToString(),
                                                            keyboard: Keyboard.Numeric);

                string newTipAmount = await DisplayPromptAsync("Update Transaction",
                                                            "Enter new Tip:",
                                                            initialValue: transaction.Tip.ToString(),
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

                    try
                    {
                        transaction.Tip = double.Parse(newTipAmount);
                    }
                    catch (FormatException exception)
                    {

                        // Notify the user about the invalid input
                        await DisplayAlert("Invalid Input", "Please enter a valid numeric TIP amount.", "OK");
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
        }


        // Delete a transaction
        private async void OnDeleteSwipeInvoked(object sender, EventArgs e)
        {
            if (TransactionExpenseSwitch.Source.ToString().Contains("minuscash.png"))
            {
                var DeleteExpense = (SwipeItem)sender;
                var expense = (Expenses)DeleteExpense.CommandParameter;
                bool confirm = await DisplayAlert("Confirm", "Are you sure you want to delete this expense?", "Yes", "No");
                if (confirm)
                {
                    // Delete the expense from the database
                    await _expensesRepository.DeleteExpenseAsync(expense);
                    // Reload the expenses to refresh the list and total
                    LoadExpensesForVendorEvent((VendorEvents)VendorEventPicker.SelectedItem);
                }
            }

            else
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
        }


        // Event handler for the date picker to load Events for the selected date
        private async void OnDateSelected(object sender, DateChangedEventArgs e)
        {
            // Load vendor events for the selected date
            await LoadVendorEventsByDate(e.NewDate);
        }

        private void OnTransactionExpenseToggled(object sender, EventArgs e)
        {
            _isExpenseToggled = !_isExpenseToggled; // Toggle the state
            


            TransactionExpenseSwitch.Source = _isExpenseToggled ? "minuscash.png" : "addcash.png";
                    TransactionLabel.Text = _isExpenseToggled ? "Expenses" : "Sales";
                    PaymentTypeLabel.IsVisible = !_isExpenseToggled;
                    PaymentTypePicker.IsVisible = !_isExpenseToggled;
                    UserExpenseLabel.IsVisible = _isExpenseToggled;
                    ExpenseLabelEntry.IsVisible = _isExpenseToggled;
                    ExpenseListLabel.IsVisible = _isExpenseToggled;
                    ExpensesList.IsVisible = _isExpenseToggled;
                    TransactionList.IsVisible = !_isExpenseToggled;
                    TransactionListLabel.IsVisible = !_isExpenseToggled;
                    TaxDeductibleLabel.IsVisible = _isExpenseToggled;
                    TaxDeductibleSwitch.IsVisible = _isExpenseToggled;
                    FeeEstimateSwitch.IsVisible = !_isExpenseToggled;
                    FeeEstimateSwitchLabel.IsVisible = !_isExpenseToggled;
                    TipEntryLabel.IsVisible = !_isExpenseToggled;
                    TipAmountEntry.IsVisible = !_isExpenseToggled;
                    

                    if (_isExpenseToggled)
                    {
                        LoadExpensesForVendorEvent((VendorEvents)VendorEventPicker.SelectedItem);
                    }
                    else
                    {
                        LoadTransactionsForVendorEvent((VendorEvents)VendorEventPicker.SelectedItem);
                    }
                            
        }

    }
}

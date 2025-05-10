namespace Vendor_App
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Vendor_App.Models;
    using Vendor_App.Repositories;
    using Microsoft.Maui.Controls;
    using System.Windows.Input;
    using System.ComponentModel;

    
    public partial class FinanceBreakdown : ContentPage, INotifyPropertyChanged
    {
        private readonly IVendorEventRepository _vendorEventRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IExpensesRepository _expensesRepository;


        public ObservableCollection<VendorEvents> Events { get; set; }
        public ObservableCollection<VendorEvents> SelectedEvents { get; set; }
        public ObservableCollection<Transaction> DisplayedTransactions { get; set; }
        
        public ObservableCollection<Expenses> DisplayedExpenses { get; set; }

        private bool _isRefreshing;
        private bool _isFeeEstimateEnabled;


        private bool isEventCollectionVisible = false;
        private bool isTransactionListVisible = false;

        public ICommand RefreshCommand { get; }

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

        public FinanceBreakdown()
        {
            InitializeComponent();

            // Initialize repositories
            var databaseConnection = new DatabaseConnection();
            _vendorEventRepository = databaseConnection.EventDatabaseConnection();
            _transactionRepository = databaseConnection.VendorDatabaseConnection();
            _expensesRepository = databaseConnection.ExpensesDatabaseConnection();
            // Initialize collections
            Events = new ObservableCollection<VendorEvents>();
            SelectedEvents = new ObservableCollection<VendorEvents>();
            DisplayedTransactions = new ObservableCollection<Transaction>();
            DisplayedExpenses = new ObservableCollection<Expenses>(); // Initialize expenses collection

            // Initialize RefreshCommand
            RefreshCommand = new Command(async () => await RefreshCommandAsync());

            BindingContext = this;

            // Load initial events
            LoadAllEvents();
        }

        
        private async Task RefreshCommandAsync()
        {
            IsRefreshing = true;

            try
            {
                // Clear all collections
                Events.Clear();
                SelectedEvents.Clear();
                DisplayedTransactions.Clear();
                DisplayedExpenses.Clear();
            
                // Reset financial totals
                TotalSales = 0;
                TotalExpenses = 0;
                TotalProcessingFees = 0;
                TotalVendorFees = 0;
                TotalTaxDeductibleExpenses = 0;
                NetProfit = 0;
                TaxableIncome = 0;

                // Clear the selection in the EventCollectionView
                if (EventCollectionView != null)
                {
                    EventCollectionView.SelectedItems?.Clear();
                }

                // Notify UI of all changes
                OnPropertyChanged(nameof(Events));
                OnPropertyChanged(nameof(SelectedEvents));
                OnPropertyChanged(nameof(DisplayedTransactions));
                OnPropertyChanged(nameof(DisplayedExpenses));

                // Reload all events
                await LoadAllEvents();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to refresh data: {ex.Message}", "OK");
            }
            finally
            {
                IsRefreshing = false;
            }
        }
        private async Task LoadAllEvents()
        {
            try
            {
                var allEvents = await _vendorEventRepository.GetAllVendorEventsAsync();
                Events.Clear();
                foreach (var vendorEvent in allEvents)
                {
                    Events.Add(vendorEvent);
                }
                Console.WriteLine($"Loaded {allEvents.Count()} events.");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load events: {ex.Message}", "OK");
            }
        }

        private async Task LoadTransactionsForSelectedEvents(List<VendorEvents> selectedEvents)
        {
            try
            {
                DisplayedTransactions.Clear();
                TotalSales = 0;
                TotalProcessingFees = 0;

                foreach (var vendorEvent in selectedEvents)
                {
                    var transactions = await _transactionRepository.GetTransactionsByVendorEventAsync(vendorEvent.VendorEventId);
                    foreach (var transaction in transactions)
                    {
                        transaction.ProcessingFee = (IsFeeEstimateEnabled && transaction.ProcessingFee > 0.00) 
                            ? transaction.ProcessingFee 
                            : 0;
                        DisplayedTransactions.Add(transaction);
                    
                        TotalSales += transaction.Amount;
                        TotalProcessingFees += transaction.ProcessingFee;
                    }
                }

                OnPropertyChanged(nameof(DisplayedTransactions));
                OnPropertyChanged(nameof(TotalSales));
                OnPropertyChanged(nameof(TotalProcessingFees));
                CalculateFinancials();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load transactions: {ex.Message}", "OK");
            }
        }
        
        private async Task LoadExpensesForSelectedEvents(List<VendorEvents> selectedEvents)
        {
            try
            {
                DisplayedExpenses.Clear();
                TotalExpenses = 0;
                TotalTaxDeductibleExpenses = 0;

                foreach (var vendorEvent in selectedEvents)
                {
                    var expenses = await _expensesRepository.GetExpensesForEventAsync(vendorEvent.VendorEventId);
                    foreach (var expense in expenses)
                    {
                        DisplayedExpenses.Add(expense);
                        TotalExpenses += expense.Amount;

                        if (expense.IsTaxDeductible)
                        {
                            TotalTaxDeductibleExpenses += expense.Amount;
                        }
                    }
                }

                OnPropertyChanged(nameof(DisplayedExpenses));
                CalculateFinancials(); // Recalculate Net Profit
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load expenses: {ex.Message}", "OK");
            }
        }
        
        private void CalculateFinancials()
        {
            NetProfit = TotalSales -  (TotalExpenses + TotalVendorFees); // Revenue minus expenses
            TaxableIncome = TotalSales - TotalTaxDeductibleExpenses; // Taxable earnings

            // Ensure UI updates
            OnPropertyChanged(nameof(NetProfit));
            OnPropertyChanged(nameof(TaxableIncome));
            OnPropertyChanged(nameof(TotalSales));
            OnPropertyChanged(nameof(TotalExpenses));
            OnPropertyChanged(nameof(TotalProcessingFees));
            OnPropertyChanged(nameof(TotalTaxDeductibleExpenses));
        }

        private async Task LoadVendorFeesForSelectedEvents(List<VendorEvents> selectedEvents)
        {
            double totalVenFees = 0;

            foreach (var vendorEvent in selectedEvents)
            {
                try
                {
                    double vendorFee = await _vendorEventRepository.GetFeeForVendorEventAsync(vendorEvent);
                    Console.WriteLine($"Fee for event {vendorEvent.VendorEventId}: {vendorFee}");
                    totalVenFees += vendorFee;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error retrieving fee for event {vendorEvent.VendorEventId}: {ex.Message}");
                    await DisplayAlert("Error", $"Failed to retrieve fee for event {vendorEvent.VendorEventId}: {ex.Message}", "OK");
                }
            }

            TotalVendorFees = totalVenFees;  // 🔥 Update the property
        }

        private async Task<double> LoadProcessingFeesForSelectedEvents(List<VendorEvents> selectedEvents)
        {
            double totalProcessingFees = 0;

            foreach (var vendorEvent in selectedEvents)
            {
                try
                {
                    var processingFees = await _transactionRepository.GetProcessingFeesForVendorEventAsync(vendorEvent.VendorEventId);
                    totalProcessingFees += processingFees.Sum();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error retrieving processing fee for event {vendorEvent.VendorEventId}: {ex.Message}");
                    await DisplayAlert("Error", $"Failed to retrieve processing fee for event {vendorEvent.VendorEventId}: {ex.Message}", "OK");
                }
            }

            return totalProcessingFees;
        }
        private void OnFeeEstimateClicked(object sender, EventArgs e)
        {
            IsFeeEstimateEnabled = !IsFeeEstimateEnabled;

            
        }
        public bool IsFeeEstimateEnabled
        {
            get => _isFeeEstimateEnabled;
            set
            {
                if (_isFeeEstimateEnabled != value)
                {
                    _isFeeEstimateEnabled = value;
                    OnPropertyChanged(nameof(IsFeeEstimateEnabled));
                    FeeEstimateSwitch.Source = value ? "colorcredit.png" : "greycredit.png";
                    UpdateProcessingFeeVisibility();
                    RefreshCommandAsync();
                }
            }
        }
        
        private void UpdateProcessingFeeVisibility()
        {
            if (DisplayedTransactions == null) return;

            foreach (var transaction in DisplayedTransactions)
            {
                // If the toggle is ON, keep the original processing fee; otherwise, hide it.
                transaction.ProcessingFee = IsFeeEstimateEnabled ? transaction.ProcessingFee : 0.00;
            }

            // Notify the UI that DisplayedTransactions has changed
            OnPropertyChanged(nameof(DisplayedTransactions));
        }
        

        private void OnToggleDateFilterClicked(object sender, EventArgs e)
        {
            DateFilterSection.IsVisible = !DateFilterSection.IsVisible;
        }

        private async void OnFilterClicked(object sender, EventArgs e)
        {
            try
            {
                DateTime startDate = StartDatePicker.Date;
                DateTime endDate = EndDatePicker.Date;

                EventCollectionView.SelectedItems.Clear();
                var filteredEvents = await _vendorEventRepository.GetEventsByDateRangeAsync(startDate, endDate);

                Events.Clear();
                foreach (var vendorEvent in filteredEvents)
                {
                    Events.Add(vendorEvent);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to filter events: {ex.Message}", "OK");
            }
        }

        private void OnToggleEventSelectionClicked(object sender, EventArgs e)
        {
            isEventCollectionVisible = !isEventCollectionVisible;
            EventCollectionView.IsVisible = isEventCollectionVisible;
            ToggleEventButton.Text = isEventCollectionVisible ? "Select Events ▲" : "Select Events ▼";
        }

        private void OnToggleTransactionListClicked(object sender, EventArgs e)
        {
            isTransactionListVisible = !isTransactionListVisible;
            TransactionListView.IsVisible = isTransactionListVisible;
            ToggleTransactionButton.Text = isTransactionListVisible ? "Transactions for Selected Events ▲" : "Transactions for Selected Events ▼";
        }
        
        private void OnToggleExpensesListClicked(object sender, EventArgs e)
        {
            ExpensesListView.IsVisible = !ExpensesListView.IsVisible;
            ToggleExpensesButton.Text = ExpensesListView.IsVisible ? "Expenses for Selected Events ▲" : "Expenses for Selected Events ▼";
        }

        private async void OnEventsSelected(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // Clear existing data first
                DisplayedTransactions.Clear();
                DisplayedExpenses.Clear();
                SelectedEvents.Clear();
            
                // Reset financial totals
                TotalSales = 0;
                TotalExpenses = 0;
                TotalProcessingFees = 0;
                TotalVendorFees = 0;
                TotalTaxDeductibleExpenses = 0;
                NetProfit = 0;
                TaxableIncome = 0;

                // Add the newly selected events
                foreach (var selectedEvent in e.CurrentSelection)
                {
                    if (selectedEvent is VendorEvents vendorEvent)
                    {
                        SelectedEvents.Add(vendorEvent);
                    }
                }

                // Only load new data if there are selected events
                if (SelectedEvents.Any())
                {
                    var selectedEventList = SelectedEvents.ToList();
                    await LoadTransactionsForSelectedEvents(selectedEventList);
                    await LoadExpensesForSelectedEvents(selectedEventList);
                    await LoadVendorFeesForSelectedEvents(selectedEventList);
                }

                // Ensure UI is updated
                OnPropertyChanged(nameof(DisplayedTransactions));
                OnPropertyChanged(nameof(DisplayedExpenses));
                OnPropertyChanged(nameof(SelectedEvents));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load data: {ex.Message}", "OK");
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        
        private double _totalSales;
        private double _totalExpenses;
        private double _netProfit;
        private double _totalProcessingFees;
        private double _totalTaxDeductibleExpenses;
        private double _taxableIncome;
        private double _totalVendorFees;
        public double TotalVendorFees
        {
            get => _totalVendorFees;
            set { _totalVendorFees = value; OnPropertyChanged(nameof(TotalVendorFees)); }
        }
        public double TotalSales
        {
            get => _totalSales;
            set { _totalSales = value; OnPropertyChanged(nameof(TotalSales)); }
        }

        public double TotalExpenses
        {
            get => _totalExpenses;
            set { _totalExpenses = value; OnPropertyChanged(nameof(TotalExpenses)); }
        }

        public double NetProfit
        {
            get => _netProfit;
            set { _netProfit = value; OnPropertyChanged(nameof(NetProfit)); }
        }

        public double TotalProcessingFees
        {
            get => _totalProcessingFees;
            set { _totalProcessingFees = value; OnPropertyChanged(nameof(TotalProcessingFees)); }
        }

        public double TotalTaxDeductibleExpenses
        {
            get => _totalTaxDeductibleExpenses;
            set { _totalTaxDeductibleExpenses = value; OnPropertyChanged(nameof(TotalTaxDeductibleExpenses)); }
        }

        public double TaxableIncome
        {
            get => _taxableIncome;
            set { _taxableIncome = value; OnPropertyChanged(nameof(TaxableIncome)); }
        }
    }
    
}

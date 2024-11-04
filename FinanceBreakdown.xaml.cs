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

    public partial class FinanceBreakdown : ContentPage
    {
        private ITransactionRepository _transactionRepository;
        private IVendorEventRepository _vendorEventRepository;

        public ObservableCollection<VendorEvents> Events { get; set; }
        public ObservableCollection<Transaction> DisplayedTransactions { get; set; }

        private bool isEventCollectionVisible = false;
        private bool isTransactionListVisible = false;

        public FinanceBreakdown()
        {
            InitializeComponent();

            // Initialize repositories and collections
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "transactions.db3");
            _transactionRepository = new SQLiteTransactionRepository(dbPath);

            string eventsDbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "vendorEvents.db3");
            _vendorEventRepository = new SQLiteVendorEventRepository(eventsDbPath);

            Events = new ObservableCollection<VendorEvents>();
            DisplayedTransactions = new ObservableCollection<Transaction>();

            BindingContext = this;

            // Load all events for selection
            LoadAllEvents();
        }

        private async void LoadAllEvents()
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

        // Toggle visibility for Event Collection
        private void OnToggleEventSelectionClicked(object sender, EventArgs e)
        {
            isEventCollectionVisible = !isEventCollectionVisible;
            EventCollectionView.IsVisible = isEventCollectionVisible;
            ToggleEventButton.Text = isEventCollectionVisible ? "Select Events ?" : "Select Events ?";
        }

        // Toggle visibility for Transaction List
        private void OnToggleTransactionListClicked(object sender, EventArgs e)
        {
            isTransactionListVisible = !isTransactionListVisible;
            TransactionListView.IsVisible = isTransactionListVisible;
            ToggleTransactionButton.Text = isTransactionListVisible ? "Transactions for Selected Events ?" : "Transactions for Selected Events ?";
        }

        private async void OnEventsSelected(object sender, SelectionChangedEventArgs e)
        {
            // Get the selected events
            var selectedEvents = e.CurrentSelection.Cast<VendorEvents>().ToList();
            await LoadTransactionsForSelectedEvents(selectedEvents);
        }

        private async Task LoadTransactionsForSelectedEvents(List<VendorEvents> selectedEvents)
        {
            try
            {
                DisplayedTransactions.Clear();
                double totalIncome = 0;
                double totalExpenses = 0;

                foreach (var vendorEvent in selectedEvents)
                {
                    // Get transactions for each selected event
                    var transactions = await _transactionRepository.GetTransactionsByVendorEventAsync(vendorEvent.VendorEventId);

                    foreach (var transaction in transactions)
                    {
                        DisplayedTransactions.Add(transaction);

                        // Calculate income and expenses
                        if (transaction.Amount > 0)
                        {
                            totalIncome += transaction.Amount;
                        }
                        else
                        {
                            totalExpenses += transaction.Amount;
                        }
                    }
                }

                // Update the financial totals
                TotalIncomeLabel.Text = $"Total Income: {totalIncome:C}";
       
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load transactions: {ex.Message}", "OK");
            }
        }
    }
}

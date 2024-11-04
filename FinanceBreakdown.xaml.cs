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
            ToggleEventButton.Text = isEventCollectionVisible ? "Select Events ▲" : "Select Events ▼";
        }

        // Toggle visibility for Transaction List
        private void OnToggleTransactionListClicked(object sender, EventArgs e)
        {
            isTransactionListVisible = !isTransactionListVisible;
            TransactionListView.IsVisible = isTransactionListVisible;
            ToggleTransactionButton.Text = isTransactionListVisible ? "Transactions for Selected Events ▲" : "Transactions for Selected Events ▼";
        }

        private async void OnEventsSelected(object sender, SelectionChangedEventArgs e)
        {
            // Get the selected events
            var selectedEvents = e.CurrentSelection.Cast<VendorEvents>().ToList();
            await LoadFeesForSelectedEvents(selectedEvents);
            await LoadTransactionsForSelectedEvents(selectedEvents);
        }

        private async Task<double> LoadFeesForSelectedEvents(List<VendorEvents> selectedEvents)
        {
            double totalFees = 0;

            foreach (var vendorEvent in selectedEvents)
            {
                try
                {
                    // Attempt to get the fee for the current event
                    float vendorFee = await _vendorEventRepository.GetFeeForVendorEventAsync(vendorEvent);
                    Console.WriteLine($"Fee for event {vendorEvent.VendorEventId}: {vendorFee}");
                    totalFees += vendorFee;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error retrieving fee for event {vendorEvent.VendorEventId}: {ex.Message}");
                    // Optionally, you could display an alert if a fee fails to load, or skip to the next
                    await DisplayAlert("Error", $"Failed to retrieve fee for event {vendorEvent.VendorEventId}: {ex.Message}", "OK");
                }
            }

            Console.WriteLine($"Total fees for selected events: {totalFees}");
            return totalFees;
        }


        private async Task LoadTransactionsForSelectedEvents(List<VendorEvents> selectedEvents)
        {
            try
            {
                DisplayedTransactions.Clear();
                double totalIncome = 0;

                double totalFees = await LoadFeesForSelectedEvents(selectedEvents);

                

                foreach (var vendorEvent in selectedEvents)
                {
                    var transactions = await _transactionRepository.GetTransactionsByVendorEventAsync(vendorEvent.VendorEventId);

                    foreach (var transaction in transactions)
                    {
                        DisplayedTransactions.Add(transaction);
                            totalIncome += transaction.Amount;

                    }
                }
                double FinalTotal = totalIncome - totalFees;
                TotalFeesLabel.Text = $"Total for all Fees: {totalFees:C}";
                SubTotalIncomeLabel.Text = $"Sub Total: {totalIncome:C}";
                TotalIncomeLabel.Text = $"Final Total: {FinalTotal:C}";

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load transactions: {ex.Message}", "OK");
            }
        }
    }
}

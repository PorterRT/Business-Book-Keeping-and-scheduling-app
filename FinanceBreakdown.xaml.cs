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

        public ObservableCollection<VendorEvents> Events { get; set; }
        public ObservableCollection<VendorEvents> SelectedEvents { get; set; }
        public ObservableCollection<Transaction> DisplayedTransactions { get; set; }

        private bool _isRefreshing;
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

            // Initialize collections
            Events = new ObservableCollection<VendorEvents>();
            SelectedEvents = new ObservableCollection<VendorEvents>();
            DisplayedTransactions = new ObservableCollection<Transaction>();

            // Initialize RefreshCommand
            RefreshCommand = new Command(async () => await RefreshCommandAsync());

            BindingContext = this;

            // Load initial events
            LoadAllEvents();
        }

        private async Task RefreshCommandAsync()
        {
            IsRefreshing = true; // Start refreshing state

            try
            {
                // Reload all events
                await LoadAllEvents();

                // If events are selected, reload transactions
                if (SelectedEvents.Any())
                {
                    var selectedEvents = SelectedEvents.ToList();
                    await LoadTransactionsForSelectedEvents(selectedEvents);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to refresh data: {ex.Message}", "OK");
            }
            finally
            {
                IsRefreshing = false; // Stop refreshing state
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

                foreach (var vendorEvent in selectedEvents)
                {
                    var transactions = await _transactionRepository.GetTransactionsByVendorEventAsync(vendorEvent.VendorEventId);
                    foreach (var transaction in transactions)
                    {
                        DisplayedTransactions.Add(transaction);
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load transactions: {ex.Message}", "OK");
            }
        }

        private async Task<double> LoadVendorFeesForSelectedEvents(List<VendorEvents> selectedEvents)
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

            return totalVenFees;
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

        private async void OnEventsSelected(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                SelectedEvents.Clear();
                foreach (var selectedEvent in e.CurrentSelection)
                {
                    SelectedEvents.Add(selectedEvent as VendorEvents);
                }

                var selectedEventList = SelectedEvents.ToList();
                await LoadTransactionsForSelectedEvents(selectedEventList);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load transactions: {ex.Message}", "OK");
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

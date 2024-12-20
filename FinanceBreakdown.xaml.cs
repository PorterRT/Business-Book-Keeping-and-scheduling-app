﻿namespace Vendor_App
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

        private readonly IVendorEventRepository _vendorEventRepository;
        private readonly ITransactionRepository _transactionRepository;

        public ObservableCollection<VendorEvents> Events { get; set; }
        
        public ObservableCollection<VendorEvents> SelectedEvents { get; set; }

        public ObservableCollection<Transaction> DisplayedTransactions { get; set; }

        private bool isEventCollectionVisible = false;
        private bool isTransactionListVisible = false;

        public FinanceBreakdown()
        {
            InitializeComponent();

            // Initialize database connection
            var databaseConnection = new DatabaseConnection();

            // Initialize repositories
            _transactionRepository = databaseConnection.VendorDatabaseConnection();
            _vendorEventRepository = databaseConnection.EventDatabaseConnection();
            // Initialize collections
            Events = new ObservableCollection<VendorEvents>();
            SelectedEvents = new ObservableCollection<VendorEvents>();
            DisplayedTransactions = new ObservableCollection<Transaction>();

            BindingContext = this;

            // Load all events for selection
            LoadAllEvents();
        }
        // Toggle Date Filter Section
        private void OnToggleDateFilterClicked(object sender, EventArgs e)
        {
            DateFilterSection.IsVisible = !DateFilterSection.IsVisible;
        }
        private async void OnFilterClicked(object sender, EventArgs e)
        {
            DateTime startDate = StartDatePicker.Date;
            DateTime endDate = EndDatePicker.Date;
            
            // Clear current selection to reset calculations
            EventCollectionView.SelectedItems.Clear();

            var filteredEvents = await _vendorEventRepository.GetEventsByDateRangeAsync(startDate, endDate);
            
            Events.Clear();
            
            foreach (var vendorEvent in filteredEvents)
            {
                Events.Add(vendorEvent);
            }
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
            try
            {
                // Synchronize SelectedEvents with current selections
                SelectedEvents.Clear();
                foreach (var selectedEvent in e.CurrentSelection)
                {
                    SelectedEvents.Add(selectedEvent as VendorEvents);
                }

                // Load transactions for the selected events
                var selectedEventList = SelectedEvents.ToList();
                await LoadTransactionsForSelectedEvents(selectedEventList);
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
                    // Attempt to get the fee for the current event
                    double vendorFee = await _vendorEventRepository.GetFeeForVendorEventAsync(vendorEvent);
                    Console.WriteLine($"Fee for event {vendorEvent.VendorEventId}: {vendorFee}");
                    totalVenFees += vendorFee;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error retrieving fee for event {vendorEvent.VendorEventId}: {ex.Message}");
                    // Optionally, you could display an alert if a fee fails to load, or skip to the next
                    await DisplayAlert("Error", $"Failed to retrieve fee for event {vendorEvent.VendorEventId}: {ex.Message}", "OK");
                }
            }

            Console.WriteLine($"Total fees for selected events: {totalVenFees}");
            return totalVenFees;
        }
        private async Task<double> LoadProcessingFeesForSelectedEvents(List<VendorEvents> selectedEvents){
            double totalProcessingFees = 0;

            foreach (var vendorEvent in selectedEvents)
            {
                try
                {
                    List<double> ProcessingFee = await _transactionRepository.GetProcessingFeesForVendorEventAsync(vendorEvent.VendorEventId);
                    Console.WriteLine($"Processing Fee for event {vendorEvent.VendorEventId}: {ProcessingFee}");
                    totalProcessingFees = ProcessingFee.Sum();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error retrieving processing fee for event {vendorEvent.VendorEventId}: {ex.Message}");
                    await DisplayAlert("Error", $"Failed to retrieve processing fee for event {vendorEvent.VendorEventId}: {ex.Message}", "OK");
                }

        }
        return totalProcessingFees;
        }



        private async Task LoadTransactionsForSelectedEvents(List<VendorEvents> selectedEvents)
        {
            try
            {
                // Clear existing transactions
                DisplayedTransactions.Clear();

                // Remove duplicate events
                selectedEvents = selectedEvents.Distinct().ToList();

                double totalIncome = 0;

                // Calculate fees
                double totalVenFees = await LoadVendorFeesForSelectedEvents(selectedEvents);
                double totalProcessingFees = await LoadProcessingFeesForSelectedEvents(selectedEvents);
                double totalFees = totalVenFees + totalProcessingFees;

                foreach (var vendorEvent in selectedEvents)
                {
                    var transactions = await _transactionRepository.GetTransactionsByVendorEventAsync(vendorEvent.VendorEventId);

                    foreach (var transaction in transactions)
                    {
                        // Avoid adding duplicate transactions
                        if (!DisplayedTransactions.Contains(transaction))
                        {
                            DisplayedTransactions.Add(transaction);
                            totalIncome += transaction.Amount;
                        }
                    }
                }

                // Calculate final total
                double finalTotal = totalIncome - totalFees;

                // Update labels
                TotalEventFeesLabel.Text = $"Total for all Event Fees: {totalVenFees:C}";
                TotalProcessingFeesLabel.Text = $"Total for Estimated Processing Fees: {totalProcessingFees:C}";
                TotalFeesLabel.Text = $"Total for all Fees: {totalFees:C}";
                SubTotalIncomeLabel.Text = $"Sub Total: {totalIncome:C}";
                TotalIncomeLabel.Text = $"Final Total: {finalTotal:C}";
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load transactions: {ex.Message}", "OK");
            }
        }
}
}

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Vendor_App.Models;
using Vendor_App.Repositories;

namespace Vendor_App
{
    public partial class VendorEventViewer : ContentPage
    {
        private readonly IVendorEventRepository _vendorEventRepository;
        private ObservableCollection<VendorEvents> _events;

        public VendorEventViewer()
        {
            InitializeComponent();

            // Initialize the repository using the same approach as VendorEventManager
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "vendorEvents.db3");
            _vendorEventRepository = new SQLiteVendorEventRepository(dbPath);

            // Initialize the ObservableCollection to hold the events
            _events = new ObservableCollection<VendorEvents>();
            EventsListView.ItemsSource = _events; // Bind the ListView to the ObservableCollection
        }

        // Handle date selection from the DatePicker
        private async void OnDateSelected(object sender, DateChangedEventArgs e)
        {
            DateTime selectedDate = e.NewDate;
            await LoadVendorEventsByDateAsync(selectedDate);
        }

        // Load vendor events for the selected date
        private async Task LoadVendorEventsByDateAsync(DateTime date)
        {
            try
            {
                // Clear the previous events
                _events.Clear();

                // Fetch the events for the selected date using the repository
                var events = await _vendorEventRepository.GetVendorEventsByDateAsync(date);

                // Add each fetched event to the ObservableCollection
                foreach (var vendorEvent in events)
                {
                    _events.Add(vendorEvent);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load events: {ex.Message}", "OK");
            }
        }
    }
}
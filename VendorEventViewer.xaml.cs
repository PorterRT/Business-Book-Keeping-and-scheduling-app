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
        public ObservableCollection<VendorEvents> Events { get; set; }

        public Command<VendorEvents> DeleteCommand { get; }
        public Command<VendorEvents> UpdateCommand { get; }

        public VendorEventViewer()
        {
            InitializeComponent();
            this.BindingContext = this;

            try
            {
                // Initialize the repository using the same approach as VendorEventManager
                string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "vendorEvents.db3");
                _vendorEventRepository = new SQLiteVendorEventRepository(dbPath);

                // Initialize the ObservableCollection to hold the events
                Events = new ObservableCollection<VendorEvents>();
                EventsListView.ItemsSource = Events; // Bind the ListView to the ObservableCollection

                DeleteCommand = new Command<VendorEvents>(OnDeleteVendorEvent); // Initialize the DeleteCommand

                UpdateCommand = new Command<VendorEvents>(OnUpdateVendorEvent);

                // Load all events to display
              //  LoadAllVendorEventsAsync();
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", $"Failed to initialize: {ex.Message}", "OK");
            }
        }

        private async void OnFilterClicked(object sender, EventArgs e)
        {
            DateTime startDate = StartDatePicker.Date;
            DateTime endDate = EndDatePicker.Date;

            var filteredEvents = await _vendorEventRepository.GetEventsByDateRangeAsync(startDate, endDate);
            Events.Clear();
            foreach (var vendorEvent in filteredEvents)
            {
                Events.Add(vendorEvent);
            }
        }

        // Method to load all vendor events
        private async Task LoadAllVendorEventsAsync()
        {
            try
            {
                // Clear the previous events
                Events.Clear();

                // Fetch all events using the repository
                var allEvents = await _vendorEventRepository.GetAllVendorEventsAsync();

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
        private async void OnEventTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null) return;

            // Get the selected event
            var selectedEvent = e.Item as VendorEvents;

            // Ensure the event is valid
            if (selectedEvent == null)
                return;

            // Display the event details
            await DisplayAlert(
                "Event Details",
                $"Name: {selectedEvent.Name}\n" +
                $"Date: {selectedEvent.EventDate.ToShortDateString()}\n" +
                $"Address: {selectedEvent.Address}\n" +
                $"Setup Time: {selectedEvent.SetupTime.ToShortTimeString()}\n" +
                $"Start Time: {selectedEvent.StartTime.ToShortTimeString()}\n" +
                $"End Time: {selectedEvent.EndTime.ToShortTimeString()}\n" +
                $"Fee: {selectedEvent.Fee}\n" +
                $"Recurring: {selectedEvent.Recurring}\n" +
                $"Fee Is Paid: {selectedEvent.FeePaid}\n" +
                $"Email: {selectedEvent.Email}\n" +
                $"Phone Number: {selectedEvent.PhoneNumber}\n" +
                $"Description: {selectedEvent.Description}",
                "OK"
            );

            // Attempt to add to calendar (this part might fail without permissions)
            try
            {
                OnAddToCalendar(selectedEvent);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not add to calendar: {ex.Message}", "OK");
            }

            // Deselect the item (so the user can tap it again)
            ((ListView)sender).SelectedItem = null;
        }


        private async void OnDeleteVendorEvent(VendorEvents vendorEvent)
        {
            bool confirm = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete {vendorEvent.Name}?", "Yes", "No");
            if (confirm)
            {
                try
                {
                    // Delete the event from the database
                    await _vendorEventRepository.DeleteVendorEventAsync(vendorEvent);

                    // Remove the event from the ObservableCollection to update the UI
                    Events.Remove(vendorEvent);

                    await DisplayAlert("Success", "Event deleted successfully", "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Failed to delete event: {ex.Message}", "OK");
                }
            }
        }
        private async void OnUpdateVendorEvent(VendorEvents vendorEvent)
        {
            await Navigation.PushAsync(new VendorEventManager(vendorEvent));
        }

        private async void OnAddToCalendar(VendorEvents vendorEvent)
        {
            try
            {
                // Ensure the DependencyService has been registered and implemented correctly
                var calendarService = DependencyService.Get<ICalendarService>();

                if (calendarService == null)
                {
                    await DisplayAlert("Error", "Calendar service not available.", "OK");
                    return;
                    }

            // Call the platform-specific calendar service to add the event
            await calendarService.AddEventToCalendar(
                vendorEvent.Name,                // Event title
                vendorEvent.StartTime,           // Event start date/time
                vendorEvent.EndTime,             // Event end date/time
                vendorEvent.SetupTime,           // Event setup time
                vendorEvent.StartTime,           // Event actual start time
                vendorEvent.Address              // Event location
            );

        // Notify the user of success
            await DisplayAlert("Success", "Event added to your calendar!", "OK");
        }
            catch (Exception ex)
        {
        // Show error details in an alert
        await DisplayAlert("Error", $"Failed to add event to calendar: {ex.Message}", "OK");
        }
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

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadAllVendorEventsAsync(); // Refresh the event list whenever the page appears.
        }

    }
}
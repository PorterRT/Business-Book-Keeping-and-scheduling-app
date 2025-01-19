using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Windows.Input;
using System.ComponentModel;
using Vendor_App.Models;
using Vendor_App.Repositories;


namespace Vendor_App
{
    public partial class VendorEventViewer : ContentPage
    {
        private readonly IVendorEventRepository _vendorEventRepository;
        private readonly ICalendarService _calendarService;
        public ObservableCollection<VendorEvents> Events { get; set; }

        public Command<VendorEvents> DeleteCommand { get; }
        public Command<VendorEvents> UpdateCommand { get; }
        
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

        public VendorEventViewer()
        {
            InitializeComponent();
            RefreshCommand = new Command(async () => await RefreshCommandAsync());
            this.BindingContext = this;

            try
            {
                // Initialize the database
                var databaseConnection = new DatabaseConnection();

                // Initialize Vendor Event repository
                _vendorEventRepository = databaseConnection.EventDatabaseConnection();

                // Initialize the CalendarService. this has an error but works and breaks if i delete the line so dont touch it
                _calendarService = new CalendarService();
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

        private void OnToggleDateFilterClicked(object sender, EventArgs e)
        {
            DateFilterSection.IsVisible = !DateFilterSection.IsVisible;
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
        
        private async Task RefreshCommandAsync()
        {
            IsRefreshing = true;

            try
            {
                // Refresh the Picker's data (Vendor Events)
                await LoadAllVendorEventsAsync();
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
            bool descriptionResponse = await DisplayAlert(
                "Event Details",
                $"Name: {selectedEvent.Name}\n" +
                $"Date: {selectedEvent.EventDate.ToShortDateString()}\n" +
                $"Address: {selectedEvent.Address}\n" +
                $"Setup Time: {selectedEvent.SetupTime.ToShortTimeString()}\n" +
                $"Start Time: {selectedEvent.StartTime.ToShortTimeString()}\n" +
                $"End Time: {selectedEvent.EndTime.ToShortTimeString()}\n" +
                $"Fee: {selectedEvent.Fee}\n" +
                $"Recurring: {RecurringMessage(selectedEvent)}\n" +
                $"Fee Is Paid: {FeePaidMessage(selectedEvent)}\n" +
                $"Email: {selectedEvent.Email}\n" +
                $"Phone Number: {selectedEvent.PhoneNumber}\n" +
                $"Description: {selectedEvent.Description}",
                "Add Event to Calendar?",
                "Close"
            );

            // Attempt to add to calendar (this part will fail without permissions)
            if (descriptionResponse)
            {
                    OnAddToCalendar(selectedEvent);
                // Deselect the item (so the user can tap it again)
                ((ListView)sender).SelectedItem = null;
            }
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
                    if (Events.Contains(vendorEvent))
                    {
                        Events.Remove(vendorEvent);
                    }

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
                var calendarService = _calendarService;
               var isEventAdded = await calendarService.IsEventAlreadyAdded(
                    vendorEvent.Name,
                    vendorEvent.StartTime,
                    vendorEvent.EndTime);
                {
                    // Call the platform-specific calendar service to add the event
                    await calendarService.AddEventToCalendar(
                        vendorEvent.Name, // Event title
                        vendorEvent.StartTime, // Event start date/time
                        vendorEvent.EndTime, // Event end date/time
                        vendorEvent.SetupTime, // Event setup time
                        vendorEvent.StartTime, // Event actual start time
                        vendorEvent.Address // Event location
                    );

                    // Notify the user of success
                    await DisplayAlert("Success", "Event added to your calendar!", "OK");
                }
            }
            catch (Exception ex)
            {
                // Show error details in an alert
                await DisplayAlert("Error", $"{ex.Message}", "OK");
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
        
        private string FeePaidMessage(VendorEvents vendorEvent)
        {
            if(vendorEvent.FeePaid)
            {
                string FeePaidMsg = "\u2713";
                return FeePaidMsg;
            }

            else
            {
                //string FeePaidMsg = "\u274C";
                string FeePaidMsg = "X";
                return FeePaidMsg;
            }
        }

        private string RecurringMessage(VendorEvents vendorEvent)
        {
            if (vendorEvent.Recurring)
            {
                string RecurringMsg = "\u2713";
                return RecurringMsg;
            }
            else
            {
                //string RecurringMsg = "\u274C";
                string RecurringMsg = "X";
                return RecurringMsg;
            }
        }

    }
}
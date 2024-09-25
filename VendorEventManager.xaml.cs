using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vendor_App.Models;
using Vendor_App.Repositories;

namespace Vendor_App;

public partial class VendorEventManager : ContentPage
{
    private IVendorEventRepository _vendorEventRepository;
    VendorEvents vendorEvent = new VendorEvents();
    public VendorEventManager()
    {
        InitializeComponent();
        string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "vendorEvents.db3");
        _vendorEventRepository = new SQLiteVendorEventRepository(dbPath);

        // Attach PropertyChanged event handlers to TimePickers
        EventSetupTimePicker.PropertyChanged += TimePicker_PropertyChanged;
        EventStartTimePicker.PropertyChanged += TimePicker_PropertyChanged;
        EventEndTimePicker.PropertyChanged += TimePicker_PropertyChanged;
    }

    // General event handler for detecting time changes in TimePickers
    private void TimePicker_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TimePicker.Time))
        {
            var picker = sender as TimePicker;
            Console.WriteLine($"Time changed to: {picker.Time}");
            // Handle the time change as needed
        }
    }

    private void OnDateSelected(object sender, DateChangedEventArgs e)
    {
        // Load the VendorEvent for the selected date
        LoadVendorEventsByDateAsync(e.NewDate);
    }

    private async void LoadVendorEventsByDateAsync(DateTime date)
    {
        // Convert DateTime to DateOnly since repository expects DateOnly type

        // Fetch events for the selected date using the repository
        var events = await _vendorEventRepository.GetVendorEventsByDateAsync(date);

        // Process the fetched events as needed (e.g., display them in the UI)
        foreach (var vendorEvent in events)
        {
            Console.WriteLine($"Event: {vendorEvent.Name} on {vendorEvent.EventDate}");
            // Add more logic here if you want to display these events in the UI
        }
    }

    private void OnRecurringToggled(object sender, ToggledEventArgs e)
    {
        // Handle the toggled state change
        bool isToggled = e.Value;
        Console.WriteLine($"Recurring switch toggled: {isToggled}");
    }


    private async void OnAddVendorEventClicked(object sender, EventArgs e)
    {
        if (float.TryParse(EventFeeEntry.Text, out float fee))
        {
            try
            {
                if (EventNameEntry.Text == null)
                {
                    throw new Exception("Please enter a name");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                var vendorEvent = new VendorEvents
                {
                    Name = EventNameEntry.Text,
                    Fee = fee,
                    EventDate = VendorEventDatePicker.Date,
                    SetupTime = VendorEventDatePicker.Date.Add(EventSetupTimePicker.Time),
                    StartTime = VendorEventDatePicker.Date.Add(EventStartTimePicker.Time),
                    EndTime = VendorEventDatePicker.Date.Add(EventEndTimePicker.Time),
                    Recurring = RecurringSwitch.IsToggled
                };
                await _vendorEventRepository.SaveVendorEventAsync(vendorEvent);
                await DisplayAlert("Success", "Vendor Event added successfully", "OK");
                ClearForm();
            }
        }
        else
        {
            await DisplayAlert("Error", "Please enter a valid fee", "OK");
        }
    }
    //clear form after adding a vendor event
    private void ClearForm()
    {
        EventNameEntry.Text = "";
        EventFeeEntry.Text = "";
        VendorEventDatePicker.Date = DateTime.Today;
        EventSetupTimePicker.Time = TimeSpan.Zero;
        EventStartTimePicker.Time = TimeSpan.Zero;
        EventEndTimePicker.Time = TimeSpan.Zero;
        RecurringSwitch.IsToggled = false;
    }
}

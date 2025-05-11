using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Vendor_App.Models;
using Vendor_App.Repositories;

namespace Vendor_App
{
    public partial class VendorEventManager : ContentPage
    {
        private readonly IVendorEventRepository _vendorEventRepository;

        // Declare a field to store the current event being edited or added.
        private VendorEvents _currentVendorEvent; 

        // Constructor that accepts an optional VendorEvents parameter.
        public VendorEventManager(VendorEvents vendorEvent = null) 
        {
            InitializeComponent();
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "vendorEvents.db3");
            _vendorEventRepository = new SQLiteVendorEventRepository(dbPath);

            // Attach PropertyChanged event handlers to TimePickers
            EventSetupTimePicker.PropertyChanged += TimePicker_PropertyChanged;
            EventStartTimePicker.PropertyChanged += TimePicker_PropertyChanged;
            EventEndTimePicker.PropertyChanged += TimePicker_PropertyChanged;

            // Check if a VendorEvents object was passed.
            if (vendorEvent != null) //
            {
                _currentVendorEvent = vendorEvent;
                PopulateFormForEditing(vendorEvent); 
            }
            else
            {
                _currentVendorEvent = new VendorEvents(); 
            }
        }

        // Method to populate the form with the data of the event being edited.
        private void PopulateFormForEditing(VendorEvents vendorEvent) 
        {
            EventNameEntry.Text = vendorEvent.Name;
            EventFeeEntry.Text = vendorEvent.Fee.ToString();
            EventAddressEntry.Text = vendorEvent.Address;
            VendorEventDatePicker.Date = vendorEvent.EventDate;
            VendorEventEndDatePicker.Date = vendorEvent.EventEndDate;
            EventSetupTimePicker.Time = vendorEvent.SetupTime.TimeOfDay;
            EventStartTimePicker.Time = vendorEvent.StartTime.TimeOfDay;
            EventEndTimePicker.Time = vendorEvent.EndTime.TimeOfDay;
            RecurringSwitch.IsToggled = vendorEvent.Recurring;
            FeePaidSwitch.IsToggled = vendorEvent.FeePaid;
            EventEmailContact.Text = vendorEvent.Email;
            EventPhoneContact.Text = vendorEvent.PhoneNumber;
            EventDescription.Text = vendorEvent.Description;
        }

        // Event handler for when the Save button is clicked.
        private async void OnAddVendorEventClicked(object sender, EventArgs e)
        {
            if (float.TryParse(EventFeeEntry.Text, out float fee))
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(EventNameEntry.Text))
                    {
                        throw new Exception("Please enter a name");
                    }

                    // Update the current event with the form data.
                    _currentVendorEvent.Name = EventNameEntry.Text; // Update existing event.
                    _currentVendorEvent.Fee = fee;
                    _currentVendorEvent.Address = EventAddressEntry.Text;
                    _currentVendorEvent.EventDate = VendorEventDatePicker.Date;
                    _currentVendorEvent.EventEndDate = VendorEventEndDatePicker.Date;
                    _currentVendorEvent.SetupTime = VendorEventDatePicker.Date.Add(EventSetupTimePicker.Time);
                    _currentVendorEvent.StartTime = VendorEventDatePicker.Date.Add(EventStartTimePicker.Time);
                    _currentVendorEvent.EndTime = VendorEventDatePicker.Date.Add(EventEndTimePicker.Time);
                    _currentVendorEvent.Recurring = RecurringSwitch.IsToggled;
                    _currentVendorEvent.FeePaid = FeePaidSwitch.IsToggled;
                    _currentVendorEvent.Email = EventEmailContact.Text;
                    _currentVendorEvent.PhoneNumber = EventPhoneContact.Text;
                    _currentVendorEvent.Description = EventDescription.Text;

                    // Save or update the event in the database.
                    await _vendorEventRepository.SaveVendorEventAsync(_currentVendorEvent); // Save changes to existing or new event.
                    await DisplayAlert("Success", "Vendor Event saved successfully", "OK");

                    // Navigate back to the previous page after saving.
                    await Navigation.PopAsync(); // <-- CHANGE: Go back to the previous page.
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", ex.Message, "OK");
                }
            }
            else
            {
                await DisplayAlert("Error", "Please enter a valid fee", "OK");
            }
        }

        // Method to clear the form.
        private void ClearForm()
        {
            EventNameEntry.Text = "";
            EventFeeEntry.Text = "";
            EventAddressEntry.Text = "";
            VendorEventDatePicker.Date = DateTime.Today;
            VendorEventEndDatePicker.Date = DateTime.Today;
            EventSetupTimePicker.Time = TimeSpan.Zero;
            EventStartTimePicker.Time = TimeSpan.Zero;
            EventEndTimePicker.Time = TimeSpan.Zero;
            RecurringSwitch.IsToggled = false;
            FeePaidSwitch.IsToggled = false;
        }

        // Event handler for time picker changes.
        private void TimePicker_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TimePicker.Time))
            {
                var picker = sender as TimePicker;
                Console.WriteLine($"Time changed to: {picker.Time}");
            }
        }

        private void OnDateSelected(object sender, DateChangedEventArgs e)
        {
            Console.WriteLine($"Date selected: {e.NewDate}");
        }

        private void OnRecurringToggled(object sender, ToggledEventArgs e)
        {
            bool isToggled = e.Value;
            Console.WriteLine($"Recurring switch toggled: {isToggled}");
        }

        private void OnFeePaid(object sender, ToggledEventArgs e)
        {
            bool isToggled = e.Value;
            Console.WriteLine($"FeePaid Switch toggled: {isToggled}");
        }

    }
}

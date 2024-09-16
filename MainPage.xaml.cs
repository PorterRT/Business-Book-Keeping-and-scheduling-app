namespace Vendor_App
{
    using System.Collections.ObjectModel;
    using Vendor_App.Models;
public partial class MainPage : ContentPage
    {
        private ObservableCollection<Transactions> transactions = new ObservableCollection<Transactions>();
        private double total = 0;

        public MainPage()
        {
            InitializeComponent();

            TransactionList.ItemsSource = transactions;
        }
      private void OnAddTransactionClicked(object sender, EventArgs e)
        {
           
            if (double.TryParse(AmountEntry.Text, out double amount))
            {
                // get the payment type from the Picker
                string paymentType = PaymentTypePicker.SelectedItem.ToString() ?? "Unknown"; // ?? is null-coalescing operator it will return Unknown if PaymentTypePicker.SelectedItem is null
                
                // add the transaction to the list
                transactions.Add(new Transactions { paymentType = paymentType, Amount = amount });

                // Update the ListView
                TransactionList.ItemsSource = null; // Set the ItemsSource to null to force it to update/ refresh the list itself
                TransactionList.ItemsSource = transactions; // Set the ItemsSource to the updated list

                // Update the total
                total += amount;
                TotalAmountLabel.Text = $"Total: {total:C}";

                // Clear the amount entry and payment type picker
                AmountEntry.Text = string.Empty;
                PaymentTypePicker.SelectedIndex = -1; // Set the selected index to -1 to clear the selection
            }
            else
            {
                // Display an alert if the amount is invalid
                DisplayAlert("Invalid Amount", "Please enter a valid amount", "OK");
            }
        }
    }
 }


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Maui.Controls;
using Vendor_App.Models;
#if ANDROID
using Android.OS;
using Android.Media;
#endif
#if IOS
using UIKit;
using Foundation;
using Microsoft.Maui; // For FileSystem
#endif
using Microsoft.Maui.Storage;

namespace Vendor_App.Utillites
{
    public partial class CSVExport : ContentPage
    {
        private readonly List<VendorEvents> _events;
        private readonly List<Transaction> _transactions;
        private readonly List<Expenses> _expenses;
        private readonly double _totalSales;
        private readonly double _totalTips;
        private readonly double _totalExpenses;
        private readonly double _netProfit;
        private readonly double _totalProcessingFees;
        private readonly double _totalTaxDeductibleExpenses;
        private readonly double _taxableIncome;
        private readonly double _totalVendorFees;

        public CSVExport(List<VendorEvents> events, List<Transaction> transactions, List<Expenses> expenses,
            double totalSales, double totalTips, double totalExpenses, double netProfit,
            double totalProcessingFees, double totalTaxDeductibleExpenses, double taxableIncome, double totalVendorFees)
        {
            _events = events;
            _transactions = transactions;
            _expenses = expenses;
            _totalSales = totalSales;
            _totalTips = totalTips;
            _totalExpenses = totalExpenses;
            _netProfit = netProfit;
            _totalProcessingFees = totalProcessingFees;
            _totalTaxDeductibleExpenses = totalTaxDeductibleExpenses;
            _taxableIncome = taxableIncome;
            _totalVendorFees = totalVendorFees;

            ExportCsv();
        }

        private async void ExportCsv()
        {
            try
            {
                var csvBuilder = new StringBuilder();

                // --- Event Summary Section ---
                csvBuilder.AppendLine("--- Event Summary ---");
                csvBuilder.AppendLine("Event Name,Date,Location,Booth Fee");
                foreach (var e in _events)
                {
                    csvBuilder.AppendLine($"{Escape(e.Name)},{e.EventDate:yyyy-MM-dd},{Escape(e.Address)},{e.Fee}");
                }
                csvBuilder.AppendLine();

                // --- Sales Section ---
                csvBuilder.AppendLine("--- Sales ---");
                csvBuilder.AppendLine("Event Name,Sale Date,Sale Amount,Tip Amount,Processing Fee,Payment Type");
                foreach (var t in _transactions)
                {
                    var eventName = _events.Find(e => e.VendorEventId == t.VendorEventId)?.Name ?? "Unknown";
                    csvBuilder.AppendLine($"{Escape(eventName)},{t.Date:yyyy-MM-dd},{t.Amount},{t.Tip},{t.ProcessingFee},{Escape(t.paymentType)}");
                }
                csvBuilder.AppendLine();

                // --- Expenses Section ---
                csvBuilder.AppendLine("--- Expenses ---");
                csvBuilder.AppendLine("Event Name,Expense Date,Expense Amount,Expense Category,Tax Deductible");
                foreach (var exp in _expenses)
                {
                    var eventName = _events.Find(e => e.VendorEventId == exp.VendorEventId)?.Name ?? "Unknown";
                    csvBuilder.AppendLine($"{Escape(eventName)},{exp.Date:yyyy-MM-dd},{exp.Amount},{Escape(exp.Label)},{(exp.IsTaxDeductible ? "Yes" : "No")}");
                }
                csvBuilder.AppendLine();

                // --- Financial Summary ---
                csvBuilder.AppendLine("--- Financial Summary ---");
                csvBuilder.AppendLine($"Total Sales:,{_totalSales:C}");
                csvBuilder.AppendLine($"Total Tips:,{_totalTips:C}");
                csvBuilder.AppendLine($"Total Expenses:,{_totalExpenses:C}");
                csvBuilder.AppendLine($"Net Profit:,{_netProfit:C}");
                csvBuilder.AppendLine($"Total Processing Fees:,{_totalProcessingFees:C}");
                csvBuilder.AppendLine($"Total Tax Deductible Expenses:,{_totalTaxDeductibleExpenses:C}");
                csvBuilder.AppendLine($"Taxable Income:,{_taxableIncome:C}");
                csvBuilder.AppendLine($"Total Vendor Fees:,{_totalVendorFees:C}");

                string fileName = $"VendorReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                string filePath = "";

                if (DeviceInfo.Platform == DevicePlatform.Android)
                {
#if ANDROID
                    var downloadsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath;
                    filePath = Path.Combine(downloadsPath, fileName);
#endif
                }
                else if (DeviceInfo.Platform == DevicePlatform.iOS)
                {
#if IOS
                    // Placeholder for iOS - saving to app's cache and showing share sheet
                    string tempPath = Path.Combine(FileSystem.CacheDirectory, fileName);
                    File.WriteAllText(tempPath, csvBuilder.ToString());

                    var url = NSUrl.FromFilename(tempPath);
                    var activityItems = new NSObject[] { url };
                    var avc = new UIActivityViewController(activityItems, null);

                    var window = GetKeyWindow();
                    if (window?.RootViewController != null)
                    {
                        await window.RootViewController.PresentViewControllerAsync(avc, true);
                    }

                    await DisplayAlert("Export Initiated", "Share sheet will appear to save the report.", "OK");
                    // On iOS, the user decides where to save, so we navigate back immediately after showing the share sheet.
                    await Navigation.PopAsync();
#else
                    filePath = Path.Combine(FileSystem.Current.AppDataDirectory, fileName); // Fallback for other non-Android platforms
#endif
                }
                else
                {
                    filePath = Path.Combine(FileSystem.Current.AppDataDirectory, fileName); // Default for other platforms
                }

#if !IOS // Don't write again on iOS as it's done in the share sheet logic
                if (DeviceInfo.Platform != DevicePlatform.iOS)
                {
                    File.WriteAllText(filePath, csvBuilder.ToString());
                    await DisplayAlert("Export Complete", $"Vendor report saved to:\n{filePath}", "OK");
                    await Navigation.PopAsync();
                }
#endif

                if (DeviceInfo.Platform == DevicePlatform.Android)
                {
#if ANDROID
                    MediaScannerConnection.ScanFile(Platform.AppContext, new string[] { filePath }, null, null);
#endif
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to export data: {ex.Message}", "OK");
            }
        }

        private string Escape(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            if (input.Contains(",") || input.Contains("\""))
            {
                return $"\"{input.Replace("\"", "\"\"")}\"";
            }
            return input;
        }

#if IOS
        public static UIWindow GetKeyWindow()
        {
            var windows = UIApplication.SharedApplication.Windows;
            foreach (var window in windows)
            {
                if (window.IsKeyWindow)
                    return window;
            }
            return null;
        }
#endif
    }
}
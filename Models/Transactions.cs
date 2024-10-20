using SQLite;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vendor_App.Models
{
    [System.ComponentModel.DataAnnotations.Schema.Table("Transactions")]
    public class Transaction
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; } // Unique identifier for the transaction in the database
        public string paymentType { get; set; } // Card or Cash 
        public double Amount { get; set; } // Amount of the transaction 

        public DateTime Date { get; set; } // Date of the transaction 

        [ForeignKey("VendorEventId")] // Foreign key to the VendorEvents table
        public int VendorEventId { get; set; } // Foreign key property

        // Add a parameterless constructor to ensure all fields have default values
        public Transaction()
        {
            paymentType = string.Empty; // Default to empty string
            Amount = 0.0; // Default to zero
            Date = DateTime.Now; // Default to current date
        }

    }
}

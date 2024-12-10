using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vendor_App.Models
{
    [Table("Expenses")]
    public class Expenses
    {
        [PrimaryKey, AutoIncrement]
        public int ExpenseId { get; set; } // Unique identifier for the Expense in the database
        public double Amount { get; set; } // Amount of the Expense
        public DateTime Date { get; set; } // Date of the Expense
        public string Label { get; set; } // Label for the Expense
        public int VendorEventId { get; set; } // Foreign key to VendorEvents

    }
}

using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vendor_App.Models
{
    [Table("Preference")]
    // If accessibility table has no data, we will use the preference data instead and modify it slightly to our needs
    public class Preference
    {
        [PrimaryKey, AutoIncrement]
        public int PreferenceId { get; set; } // Unique identifier for the preference in the database
        public int AccessibilityId { get; set; } // Foreign key to Accessibility

        public bool IsAcessibilityPopulated { get; set; } // Check and see if Accessibility table is populated

        public bool IsPreferenceDataSame { get; set; }
        private Dictionary<string, string> SetPreferenceData { set; }

    }
}
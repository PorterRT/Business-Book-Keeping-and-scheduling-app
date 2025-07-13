using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vendor_App.Models
{
    [Table("Preference")]
    /// <summary>
    /// Manages application theme switching functionality. The database exists to:
    /// 
    /// 1. Remove the currently applied theme by clearing the MergedDictionaries collection
    ///    from the application-level ResourceDictionary.
    ///    
    /// 2. Apply a new selected theme by adding the theme's ResourceDictionary instance
    ///    to the application's MergedDictionaries collection.
    /// </summary>
    /// <remarks>
    /// This implementation ensures clean theme transitions by completely removing
    /// the previous theme before applying a new one, preventing any resource conflicts
    /// or visual artifacts between theme changes.
    /// </remarks>
    public class Preference
    {
        [PrimaryKey, AutoIncrement]
        public int PreferenceId { get; set; } // Unique identifier for the preference in the database
        public bool IsPreferenceDataSame { get; set; }
        private IDictionary<string, object> SetPreferenceData { get;  set; } // This should store the preference data

    }
}
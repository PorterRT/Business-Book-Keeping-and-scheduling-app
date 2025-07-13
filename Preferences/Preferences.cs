using System.Threading.Tasks;
using Vendor_App.Repositories;
using Vendor_App.Models;

namespace Vendor_App.Preferences {
    // Internal keyword functions similarity to friend keyword in c++
    // Wherever InitializeComponent() is called, we need to call in a couple functions to update and modify the theme 
    internal class Preferences : ResourceDictionary, IPreference
    {
        // Class that will check and update the private property in IPreference
        /*public Preferences(ICollection<ResourceDictionary> userPref) {

        }*/
        public Preferences() {
            // Initialize with default color
            this["TextColor"] = Colors.White; // Default color

        }
        // Implement IPreference methods
        public Task SavePreferenceAsync(Preference pref)
        {
            // Implementation here
            throw new System.NotImplementedException();
        }

        public Task UpdatePreferenceAsync(Preference pref)
        {
            // Implementation here
            throw new System.NotImplementedException();
        }

        public Task DeletePreferenceAsync(Preference pref)
        {
            // Implementation here
            throw new System.NotImplementedException();
        }
        public void SetTextColor(Color newColor) {
            // Update the dynamic resource
            this["TextColor"] = newColor;
            
            // Force UI to update (important in MAUI)
            if (Application.Current != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(this);
                Application.Current.Resources.MergedDictionaries.Add(this);
            }
        }

        // Implement ResourceDictionary members
        // ... would need to implement all required ResourceDictionary members here
    }
}
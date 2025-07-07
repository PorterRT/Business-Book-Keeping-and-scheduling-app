using Vendor_App.Models;
namespace Vendor_App.Repositories;

    public interface IPreference
    {
        public Task SavePreferenceAsync(Preference pref);
        public Task UpdatePreferenceAsync(Preference pref);
        public Task DeletePreferenceAsync(Preference pref);

}
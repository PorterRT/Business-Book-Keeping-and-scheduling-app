using Vendor_App.Models;
namespace Vendor_App.Repositories;

public interface IPreference
{
    public Task SavePreferenceAsync(Preference pref); // This function will add the preference and the table will be updated
    public Task UpdatePreferenceAsync(Preference pref); // This function can update the theme and call in the other functions if needed 
    public Task DeletePreferenceAsync(Preference pref);

}
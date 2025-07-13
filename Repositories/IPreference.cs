using Vendor_App.Models;
namespace Vendor_App.Repositories;

public interface IPreference
{
    public Task SavePreferenceAsync(Models.Preferences pref); // This function will add the preference and the table will be updated
    public Task UpdatePreferenceAsync(Models.Preferences pref); // This function can update the theme and call in the other functions if needed 
    public Task DeletePreferenceAsync(Models.Preferences pref);

}
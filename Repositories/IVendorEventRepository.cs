using Vendor_App.Models;
namespace Vendor_App.Repositories;

    public interface IVendorEventRepository
    {
        Task SaveVendorEventAsync(VendorEvents vendorEvent); //Save a vendor event to the databse
        
        Task<VendorEvents> GetVendorEventByIdAsync(int id);
        
        Task<IEnumerable<VendorEvents>> GetAllVendorEventsAsync();
        
        Task<VendorEvents> GetVendorEventByNameAsync(string name);
        
        Task<IEnumerable<VendorEvents>> GetAllVendorEventsByNameAsync();
        
        Task DeleteVendorEventAsync(VendorEvents vendorEvent);
        
        Task UpdateVendorEventAsync(VendorEvents vendorEvent);
        
        Task<float> GetFeeForVendorEventAsync(VendorEvents vendorEvent);
        
        Task<List<VendorEvents>> GetVendorEventsByDateAsync(DateTime date);
    
    }
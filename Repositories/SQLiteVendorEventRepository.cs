using SQLite;
using Vendor_App.Models;

namespace Vendor_App.Repositories;

public class SQLiteVendorEventRepository : IVendorEventRepository
{
    private readonly SQLiteAsyncConnection _database;

    public SQLiteVendorEventRepository(string dbPath)
    {
        _database = new SQLiteAsyncConnection(dbPath);
        _database.CreateTableAsync<VendorEvents>().Wait();
        
    }
    //Save a vendor Event to the database
    public Task SaveVendorEventAsync(VendorEvents vendorEvent)
    {
        if (vendorEvent.VendorEventId != 0)
        {
            return _database.UpdateAsync(vendorEvent);
        }
        else
        {
            return _database.InsertAsync(vendorEvent);
        }

    }
    //get all vendor events by name
    public async Task<IEnumerable<VendorEvents>> GetAllVendorEventsAsync()
    {
        var vendorEventsList = await _database.Table<VendorEvents>().ToListAsync();
        return vendorEventsList.AsEnumerable();
    }
    // get a specific vendor event by name
    public Task<VendorEvents> GetVendorEventByNameAsync(string name)
    {
        return _database.Table<VendorEvents>()
            .Where(v => v.Name == name)
            .FirstOrDefaultAsync();    
    }

    public Task<List<VendorEvents>> GetVendorEventsByDateAsync(DateOnly date)
    {
        return _database.Table<VendorEvents>()
            .Where(v => v.EventDate == date)
            .ToListAsync();
    }

    public Task<float> GetFeeForVendorEventAsync(VendorEvents vendorEvent)
    {
        return _database.ExecuteScalarAsync<float>(
            "SELECT Fee FROM VendorEvents WHERE VendorEventId = ?", vendorEvent.VendorEventId);
    }

    public Task DeleteVendorEventAsync(VendorEvents vendorEvent)
    {
        return _database.DeleteAsync(vendorEvent);
    }

   
    public Task UpdateVendorEventAsync(VendorEvents vendorEvent)
    {
        return _database.UpdateAsync(vendorEvent);
    }
}
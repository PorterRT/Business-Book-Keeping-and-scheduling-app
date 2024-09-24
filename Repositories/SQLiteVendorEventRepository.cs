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
    public Task<IEnumerable<VendorEvents>> GetAllVendorEventsAsync()
    {
        throw new NotImplementedException();
    }
    // get a specific vendor event by name
    public Task<VendorEvents> GetVendorEventByNameAsync(string name)
    {
        return _database.Table<VendorEvents>()
            .Where(v => v.Name == name)
            .FirstOrDefaultAsync();    
    }

    public Task<List<VendorEvents>> GetVendorEventsByDateAsync(DateTime date)
    {
        throw new NotImplementedException();
    }

    public Task<float> GetFeeForVendorEventAsync(VendorEvents vendorEvent)
    {
      throw new NotImplementedException();
    }

    public Task DeleteVendorEventAsync(VendorEvents vendorEvent)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<VendorEvents>> GetAllVendorEventsByNameAsync()
    {
        throw new NotImplementedException();
    }

    public Task UpdateVendorEventAsync(VendorEvents vendorEvent)
    {
        throw new NotImplementedException();
    }
}
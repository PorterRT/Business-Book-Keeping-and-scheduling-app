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

    public async Task<List<VendorEvents>> GetVendorEventsByDateAsync(DateTime date)
    {
        var startDate = date.Date;
        var endDate = startDate.AddDays(1).AddTicks(-1); // End of the day

        var events = await _database.QueryAsync<VendorEvents>(
            "SELECT * FROM Vendor_events WHERE EventDate BETWEEN ? AND ?",
            startDate, endDate);

        return events;
    }




    public Task<float> GetFeeForVendorEventAsync(VendorEvents vendorEvent)
    {
        return _database.ExecuteScalarAsync<float>(
            "SELECT Fee FROM Vendor_events WHERE VendorEventId = ?", vendorEvent.VendorEventId);
    }

    public Task DeleteVendorEventAsync(VendorEvents vendorEvent)
    {
        return _database.DeleteAsync(vendorEvent);
    }

   
    public Task UpdateVendorEventAsync(VendorEvents vendorEvent)
    {
        return _database.UpdateAsync(vendorEvent);
    }
    public async Task<List<string>> GetVendorEventNamesByDateAsync(DateTime date)
    {
        // Use a raw query to select only the 'Name' column from the 'Vendor_events' table
        var query = "SELECT Name FROM Vendor_events WHERE DATE(EventDate) = DATE(?)";
        var result = await _database.QueryScalarsAsync<string>(query, date);

        return result;
    }

    public Task<List<VendorEvents>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return _database.Table<VendorEvents>()
                        .Where(e => e.EventDate >= startDate && e.EventDate <= endDate)
                        .ToListAsync();
    }

    public Task<VendorEvents> GetExpensesByEvent(VendorEvents vendorEvent)
    {
        return _database.Table<VendorEvents>()
                        .Where(e => e.VendorEventId == vendorEvent.VendorEventId)
                        .FirstOrDefaultAsync();
    }



}
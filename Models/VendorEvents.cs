using SQLite;

namespace Vendor_App.Models;

[Table ("Vendor_events")]
public class VendorEvents
{
    [PrimaryKey, AutoIncrement]
    public int VendorEventId { get; set; }
    
    public string VendorName { get; set; }
    
    public float Fee { get; set; }
    
    public DateOnly EventDate { get; set; }
    
    public DateTime SetupTime { get; set; }
    
    public DateTime StartTime { get; set; }
    
    public DateTime EndTime { get; set; }
    
    public bool Recurring { get; set; }
    
    
    
}

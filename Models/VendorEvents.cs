using SQLite;

namespace Vendor_App.Models;

[Table ("Vendor_events")]
public class VendorEvents
{
    [PrimaryKey, AutoIncrement]
    public int VendorEventId { get; set; }
    
    public string Name { get; set; }
    
    public float Fee { get; set; }

    public string Address { get; set; }
    
    public DateTime EventDate { get; set; }

    public DateTime EventEndDate {get; set;}
    
    public DateTime SetupTime { get; set; }
    
    public DateTime StartTime { get; set; }
    
    public DateTime EndTime { get; set; }
    
    public bool Recurring { get; set; }

    public bool FeePaid { get; set; }

    public string Email { get; set; }

    public string PhoneNumber { get; set; }

    public string Description { get; set; }
    
    
    
}

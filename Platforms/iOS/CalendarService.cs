using EventKit;
using Foundation;
using Vendor_App;

[assembly: Dependency(typeof(CalendarService))]
public class CalendarService : ICalendarService
{
    public async Task AddEventToCalendar(string title, DateTime startDate, DateTime endDate, DateTime setUp, DateTime startTime, string location)
{
    try
    {
        var eventStore = new EKEventStore();

        // Request access to the calendar
        var (accessGranted, error) = await eventStore.RequestFullAccessToEventsAsync();
        if (!accessGranted)
        {
            throw new Exception("Calendar access not granted.");
        }

        // Create the event
        var newEvent = EKEvent.FromStore(eventStore);
        newEvent.Title = title;
        newEvent.Notes = $"Setup Time: {setUp.ToShortTimeString()}\nStart Time: {startTime.ToShortTimeString()}";
        newEvent.Location = location;

        // Convert DateTime to NSDate
        newEvent.StartDate = (NSDate)startDate;
        newEvent.EndDate = (NSDate)endDate;

        // Set the event calendar
        newEvent.Calendar = eventStore.DefaultCalendarForNewEvents;

        // Save the event
        NSError saveError;
        eventStore.SaveEvent(newEvent, EKSpan.ThisEvent, out saveError);

        if (saveError != null)
        {
            throw new Exception($"Failed to save event: {saveError.LocalizedDescription}");
        }
    }
    catch (Exception ex)
    {
        throw new Exception($"Failed to add event to calendar: {ex.Message}", ex);
    }
    }
    };


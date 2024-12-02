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
        if (await IsEventAlreadyAdded(title, startDate, endDate))
        {
            throw new Exception("Event is already added to the calendar.");
        }
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
        newEvent.StartDate = ConvertToNSDate(startDate);
        newEvent.EndDate = ConvertToNSDate(endDate);

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
    
    private NSDate ConvertToNSDate(DateTime date)
    {
        // Ensure the DateTime has a valid DateTimeKind
        if (date.Kind == DateTimeKind.Unspecified)
        {
            // Default to Local if kind is unspecified
            date = DateTime.SpecifyKind(date, DateTimeKind.Local);
        }

        // Convert DateTime to seconds since Unix epoch and create NSDate
        var secondsSince1970 = (date.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds;
        return NSDate.FromTimeIntervalSince1970(secondsSince1970);
    }
    public async Task<bool> IsEventAlreadyAdded(string title, DateTime startDate, DateTime endDate)
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

            // Define a date range to search for events
            var startNSDate = ConvertToNSDate(startDate);
            var endNSDate = ConvertToNSDate(endDate);

            // Get the default calendar
            var calendar = eventStore.DefaultCalendarForNewEvents;

            // Fetch events in the date range
            var predicate = eventStore.PredicateForEvents(startNSDate, endNSDate, new[] { calendar });
            var matchingEvents = eventStore.EventsMatching(predicate);

            // Check if any event matches the title and start/end dates
            foreach (var existingEvent in matchingEvents)
            {
                if (existingEvent.Title == title &&
                    existingEvent.StartDate == startNSDate &&
                    existingEvent.EndDate == endNSDate)
                {
                    return true; // Event already exists
                }
            }

            return false; // Event does not exist
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to check for existing events: {ex.Message}", ex);
        }
    }

    };


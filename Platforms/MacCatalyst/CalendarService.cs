using Vendor_App;
using EventKit;
using Foundation;


[assembly: Dependency(typeof(Vendor_App.Platforms.MacCatalyst.CalendarService))]

namespace Vendor_App.Platforms.MacCatalyst
{
    public class CalendarService : ICalendarService
    {
        public async Task AddEventToCalendar(string title, DateTime startDate, DateTime endDate, DateTime setUp, DateTime startTime, string location)
        {
            var eventStore = new EKEventStore();
        var (accessGranted, error) = await eventStore.RequestFullAccessToEventsAsync();

            if (!accessGranted)
        {
            throw new Exception("Calendar access not granted.");
        }


            var newEvent = EKEvent.FromStore(eventStore);
            newEvent.Title = title;
            newEvent.Notes = $"Setup Time: {setUp.ToShortTimeString()}\nStart Time: {startTime.ToShortTimeString()}";
            newEvent.Location = location;
            newEvent.StartDate = (NSDate)startDate;
            newEvent.EndDate = (NSDate)endDate;
            newEvent.Calendar = eventStore.DefaultCalendarForNewEvents;

            NSError saveError;
            eventStore.SaveEvent(newEvent, EKSpan.ThisEvent, out saveError);

            if (saveError != null)
            {
                throw new Exception($"Failed to save event: {saveError.LocalizedDescription}");
            }
        }
    }
}

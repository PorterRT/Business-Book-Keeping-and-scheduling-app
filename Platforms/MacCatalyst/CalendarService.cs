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

            // Request calendar access
            var accessGranted = await eventStore.RequestAccessAsync(EKEntityType.Event);
            if (!accessGranted.Item1)
            {
                throw new Exception("Calendar access not granted.");
            }

            // Create a new event
            var newEvent = EKEvent.FromStore(eventStore);
            newEvent.Title = title;
            newEvent.Notes = $"Setup Time: {setUp.ToShortTimeString()}\nStart Time: {startTime.ToShortTimeString()}";
            newEvent.Location = location;
            newEvent.StartDate = ToNSDate(startDate); // Convert DateTime to NSDate
            newEvent.EndDate = ToNSDate(endDate);     // Convert DateTime to NSDate
            newEvent.Calendar = eventStore.DefaultCalendarForNewEvents;

            // Save the event
            NSError saveError;
            eventStore.SaveEvent(newEvent, EKSpan.ThisEvent, out saveError);

            if (saveError != null)
            {
                throw new Exception($"Failed to save event: {saveError.LocalizedDescription}");
            }
        }

        private NSDate ToNSDate(DateTime dateTime)
        {
            // Apple reference date: January 1, 2001
            var referenceDate = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var timeInterval = (dateTime.ToUniversalTime() - referenceDate).TotalSeconds;
            return NSDate.FromTimeIntervalSinceReferenceDate(timeInterval);
        }
    }
}

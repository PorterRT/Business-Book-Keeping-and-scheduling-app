public interface ICalendarService
{
    Task AddEventToCalendar(string title, DateTime startDate, DateTime endDate, DateTime setUp, DateTime startTime, string location);
}

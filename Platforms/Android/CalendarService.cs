using Android.Content;
using Android.Provider;
using Vendor_App;
using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel; // For Permissions API
using AndroidX.Core.Content; // AndroidX replacements
using AndroidX.Core.App;

[assembly: Dependency(typeof(CalendarService))]
public class CalendarService : ICalendarService
{
    public async Task AddEventToCalendar(string title, DateTime startDate, DateTime endDate, DateTime setUp, DateTime startTime, string location)
    {
        var context = Android.App.Application.Context;

        if (await IsEventAlreadyAdded(title, setUp, endDate))
        {
            throw new Exception("Event is already added to the calendar.");
        }
        else
        {

            // Check if permission is granted
            var permissionStatus = await Permissions.RequestAsync<Permissions.CalendarWrite>();
            if (permissionStatus != PermissionStatus.Granted)
            {
                // Permission not granted
                return;
            }


            // If permission is granted, proceed to add the event
            var calendarIntent = new Intent(Intent.ActionInsert)
                .SetData(CalendarContract.Events.ContentUri)
                .PutExtra(CalendarContract.Events.InterfaceConsts.Title, title)
                .PutExtra(CalendarContract.Events.InterfaceConsts.Description,
                    $"Setup Time: {setUp.ToShortTimeString()}\nStart Time: {startTime.ToShortTimeString()}")
                .PutExtra(CalendarContract.Events.InterfaceConsts.EventLocation, location)
                .PutExtra(CalendarContract.ExtraEventBeginTime, ConvertToMilliseconds(setUp))
                .PutExtra(CalendarContract.ExtraEventEndTime, ConvertToMilliseconds(endDate));

            calendarIntent.AddFlags(ActivityFlags.NewTask);
            context.StartActivity(calendarIntent);
        }
    }

    public async Task<bool> IsEventAlreadyAdded(string title, DateTime startDate, DateTime endDate)
    {
        var context = Android.App.Application.Context;

        // Check if permission is granted
        var permissionStatus = await Permissions.RequestAsync<Permissions.CalendarRead>();
        if (permissionStatus != PermissionStatus.Granted)
        {
            // Permission not granted
            return false;
        }

        // Define the URI for events
        var eventsUri = CalendarContract.Events.ContentUri;

        // Define the query parameters
        string[] projection = {
        CalendarContract.Events.InterfaceConsts.Title,
        CalendarContract.Events.InterfaceConsts.Dtstart,
        CalendarContract.Events.InterfaceConsts.Dtend
    };

        string selection = $"{CalendarContract.Events.InterfaceConsts.Title} = ? AND " +
                           $"{CalendarContract.Events.InterfaceConsts.Dtstart} = ? AND " +
                           $"{CalendarContract.Events.InterfaceConsts.Dtend} = ?";

        string[] selectionArgs = {
        title,
        ConvertToMilliseconds(startDate).ToString(),
        ConvertToMilliseconds(endDate).ToString()
    };

        // Perform the query
        using (var cursor = context.ContentResolver.Query(eventsUri, projection, selection, selectionArgs, null))
        {
            if (cursor != null && cursor.MoveToFirst())
            {
                // Event already exists
                return true;
            }
        }

        // Event not found
        return false;
    }



    private long ConvertToMilliseconds(DateTime dateTime)
    {
        return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
    }
}

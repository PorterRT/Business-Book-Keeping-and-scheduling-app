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
            .PutExtra(CalendarContract.ExtraEventBeginTime, ConvertToMilliseconds(startDate))
            .PutExtra(CalendarContract.ExtraEventEndTime, ConvertToMilliseconds(endDate));

        calendarIntent.AddFlags(ActivityFlags.NewTask);
        context.StartActivity(calendarIntent);
    }

    private long ConvertToMilliseconds(DateTime dateTime)
    {
        return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
    }
}

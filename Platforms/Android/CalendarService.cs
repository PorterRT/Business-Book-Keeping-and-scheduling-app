using Android.Content;
using Android.Provider;
using Vendor_App;
using System;
using DeviceInterfaces;

[assembly: Dependency(typeof(CalendarService))]
public class CalendarService : ICalendarService
{
    public async Task AddEventToCalendar(string title, DateTime startDate, DateTime endDate, DateTime setUp, DateTime startTime, string location)
    {
        var context = Android.App.Application.Context;

        // Check if permission is already granted
        if (Android.Support.V4.Content.ContextCompat.CheckSelfPermission(context, Android.Manifest.Permission.WriteCalendar) != Android.Content.PM.Permission.Granted)
        {
            // Request the permission
            Android.Support.V4.App.ActivityCompat.RequestPermissions(
                (Android.App.Activity)Platform.CurrentActivity,
                new string[] { Android.Manifest.Permission.WriteCalendar },
                1
            );

            // Wait for user response
            return;
        }

        // If permission is granted, proceed to add the event
        var calendarIntent = new Intent(Intent.ActionInsert)
            .SetData(CalendarContract.Events.ContentUri)
            .PutExtra(CalendarContract.Events.InterfaceConsts.Title, title)
            .PutExtra(CalendarContract.Events.InterfaceConsts.Description, $"Setup Time: {setUp.ToShortTimeString()}\nStart Time: {startTime.ToShortTimeString()}")
            .PutExtra(CalendarContract.Events.InterfaceConsts.EventLocation, location)
            .PutExtra(CalendarContract.EXTRA_EVENT_BEGIN_TIME, ConvertToMilliseconds(startDate))
            .PutExtra(CalendarContract.EXTRA_EVENT_END_TIME, ConvertToMilliseconds(endDate));

        calendarIntent.AddFlags(ActivityFlags.NewTask);
        context.StartActivity(calendarIntent);
    }

    private long ConvertToMilliseconds(DateTime dateTime)
    {
        return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
    }

    }
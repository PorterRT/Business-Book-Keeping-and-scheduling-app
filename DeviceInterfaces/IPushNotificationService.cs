namespace Vendor_App.DeviceInterfaces;

public interface IPushNotificationService
{
    Task NotifyByDateAsync(string title, string message, DateTime sendDate);
}
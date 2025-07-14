using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using UIKit;
namespace Vendor_App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            
#if IOS
            builder.Services.AddSingleton<ICalendarService, CalendarService>();
            SearchBarHandler.Mapper.AppendToMapping(nameof(SearchBar.BackgroundColor), (handler, view) =>
            {
                if (handler.PlatformView is UISearchBar nativeSearchBar)
                {
                    // Remove default background
                    nativeSearchBar.BackgroundImage = new UIImage();
                    nativeSearchBar.SearchTextField.BorderStyle = UITextBorderStyle.None;
        
                    if (Application.Current.RequestedTheme == AppTheme.Dark)
                    {
                        nativeSearchBar.BarTintColor = UIColor.FromRGB(28, 28, 30);
                        nativeSearchBar.SearchTextField.BackgroundColor = UIColor.FromRGB(28, 28, 30);
                        nativeSearchBar.SearchTextField.TextColor = UIColor.White;
                    }
                    else
                    {
                        nativeSearchBar.BarTintColor = UIColor.White;
                        nativeSearchBar.SearchTextField.BackgroundColor = UIColor.White;
                        nativeSearchBar.SearchTextField.TextColor = UIColor.Black;
                    }
                }
            });
            EntryHandler.Mapper.AppendToMapping("DoneButton", (handler, view) =>
            {
                if (handler.PlatformView is UITextField nativeEntry)
                {
                    var toolbar = new UIToolbar();
                    toolbar.SizeToFit();
        
                    var flexSpace = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
        
                    var doneButton = new UIBarButtonItem(
                        "Done", 
                        UIBarButtonItemStyle.Done, 
                        (s, e) =>
                        {
                            nativeEntry.ResignFirstResponder();
                        }
                    );
        
                    doneButton.TintColor = UIColor.SystemBlue;
        
                    toolbar.SetItems(new[] { flexSpace, doneButton }, false);
                    nativeEntry.InputAccessoryView = toolbar;
                }
            });
#endif

            return builder.Build();
        }
    }
}

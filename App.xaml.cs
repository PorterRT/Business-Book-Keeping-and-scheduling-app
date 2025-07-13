using Vendor_App.Preferences;
namespace Vendor_App
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            // TODO: Make an if statement here that checks to see if the app is in dark mode or not 
            // We will be calling in the Preference class here or let the class inherit it. 
            // We will then update the TextColor and sync it again

            MainPage = new AppShell();
        }
    }
}

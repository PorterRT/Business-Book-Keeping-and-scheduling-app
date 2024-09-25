namespace Vendor_App
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            new NavigationPage(new MainPage());
        }
    }
}

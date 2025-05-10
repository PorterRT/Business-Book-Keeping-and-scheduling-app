using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vendor_App.Utillites
{
    public static class ToggleUtillity
    {
        public static void ToggleImageButton(
        ImageButton button,
        ref bool isToggled, // Track state externally
        string onImage,
        string offImage,
        Action<bool>? onToggled = null) // Optional callback
        {
            isToggled = !isToggled;
            button.Source = isToggled ? onImage : offImage;
            onToggled?.Invoke(isToggled);
        }
    }
}

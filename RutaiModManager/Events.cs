using System;
using System.Windows;

namespace RutaiModManager
{
    public static class Events
    {
        public static bool CheckDate(int month, int day)
        {
            if (RegistryConfig.AllowEvents &&
                (month == 0 || month == DateTime.Now.Month) &&
                (day == 0 || day == DateTime.Now.Day))
                return true;

            return false;
        }

        public static void OnStartUp()
        {
            // 16th of March
            if (CheckDate(3, 16))
            {
                // Display Iizuka's Birthday Splash
                SplashScreen splashScreen;

                splashScreen = new("Resources/Graphics/EasterEggs/splashBirthday.png");
                splashScreen.Show(false, true);
                splashScreen.Close(TimeSpan.FromSeconds(0.5));
            }
        }
    }
}

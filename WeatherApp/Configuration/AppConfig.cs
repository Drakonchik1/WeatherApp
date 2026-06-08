using System;
using System.IO;

namespace WeatherApp.Configuration
{
    public static class AppConfig
    {
        // OpenWeatherMap API Configuration
        public static string OpenWeatherMapApiKey
        {
            get
            {
                var candidates = new[]
                {
                    Path.Combine(Directory.GetCurrentDirectory(), "apikey.txt"),
                    Path.Combine(AppContext.BaseDirectory, "apikey.txt"),
                };

                foreach (var path in candidates)
                {
                    if (File.Exists(path))
                        return File.ReadAllText(path).Trim();
                }

                return "YOUR_OPENWEATHER_API_KEY_HERE";
            }
        }
        public const string OpenWeatherMapBaseUrl = "https://api.openweathermap.org/data/2.5";
        
        // IMGW API Configuration
        public const string ImgwBaseUrl = "https://danepubliczne.imgw.pl/api/data/synop";
        
        // Application Settings
        public const int DefaultHistoricalDays = 7;
        public const string DefaultCity = "Warsaw";
        
        // Instructions for API Key Setup
        public static string GetApiKeyInstructions()
        {
            return @"
To use the OpenWeatherMap API:

1. Visit https://openweathermap.org/api
2. Sign up for a free account
3. Get your API key from the dashboard
4. Create an apikey.txt file in the WeatherApp project folder with your API key

The IMGW API is free and doesn't require an API key.
            ";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WeatherApp.Models;

namespace WeatherApp.Services
{
    public class OpenWeatherMapService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string BaseUrl = "https://api.openweathermap.org/data/2.5";

        public OpenWeatherMapService(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
        }

        public async Task<WeatherData?> GetCurrentWeatherAsync(string location)
        {
            try
            {
                var safeLocation = Uri.EscapeDataString(location);
                var url = $"{BaseUrl}/weather?q={safeLocation}&appid={_apiKey}&units=metric";
                System.Diagnostics.Debug.WriteLine($"Fetching weather from: {url}");
                
                var response = await _httpClient.GetStringAsync(url);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var weatherResponse = JsonSerializer.Deserialize<OpenWeatherMapResponse>(response, options);

                if (weatherResponse == null) 
                {
                    System.Diagnostics.Debug.WriteLine("No OpenWeatherMap data received");
                    return null;
                }

                return new WeatherData
                {
                    Time = DateTimeOffset.FromUnixTimeSeconds(weatherResponse.Dt).DateTime,
                    Location = $"{weatherResponse.Name}, {weatherResponse.Sys.Country}",
                    Temperature = weatherResponse.Main.Temp,
                    Humidity = weatherResponse.Main.Humidity,
                    Pressure = weatherResponse.Main.Pressure,
                    Cloudiness = weatherResponse.Clouds.All,
                    Rainfall = weatherResponse.Rain?.OneHour ?? 0,
                    Description = weatherResponse.Weather.FirstOrDefault()?.Description ?? "",
                    Icon = weatherResponse.Weather.FirstOrDefault()?.Icon ?? "",
                    Source = "OpenWeatherMap"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching weather from OpenWeatherMap: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        public async Task<List<WeatherData>> GetHistoricalWeatherAsync(string location, int days = 7)
        {
            try
            {
                var weatherData = new List<WeatherData>();
                var endDate = DateTime.Now;
                var startDate = endDate.AddDays(-days);

                // Note: OpenWeatherMap historical data requires a paid plan
                // For demo purposes, we'll generate some sample data
                var random = new Random();
                for (int i = 0; i < days; i++)
                {
                    var date = startDate.AddDays(i);
                    weatherData.Add(new WeatherData
                    {
                        Time = date,
                        Location = location,
                        Temperature = 15 + random.NextDouble() * 15, // 15-30°C
                        Humidity = 40 + random.NextDouble() * 40, // 40-80%
                        Pressure = 1000 + random.NextDouble() * 50, // 1000-1050 hPa
                        Cloudiness = random.NextDouble() * 100, // 0-100%
                        Rainfall = random.NextDouble() * 10, // 0-10mm
                        Description = "Partly Cloudy",
                        Icon = "02d"
                    });
                }

                return weatherData;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching historical weather: {ex.Message}");
                return new List<WeatherData>();
            }
        }
    }
}
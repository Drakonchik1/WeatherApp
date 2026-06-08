using System.Collections.Generic;
using System.Threading.Tasks;
using WeatherApp.Models;

namespace WeatherApp.Services
{
    public interface IWeatherService
    {
        Task<WeatherData?> GetCurrentWeatherAsync(string location);
        Task<List<WeatherData>> GetHistoricalWeatherAsync(string location, int days = 7);
    }
}

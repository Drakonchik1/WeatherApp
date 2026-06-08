# Weather Application

A desktop WPF weather application that integrates with OpenWeatherMap and IMGW (Polish Institute of Meteorology and Water Management) APIs to provide comprehensive weather data and analytics.

## Features

- **Dual API integration**: OpenWeatherMap and IMGW
- **Current weather**: Detailed conditions per selected city
- **Dashboard with charts**: Temperature, Humidity, Pressure, and Rainfall views
- **Date filter**: Filter loaded data by a specific day
- **Past-day samples (OpenWeatherMap)**: Fetches up to 10 fixed times for a chosen day (e.g., 00:00, 02:00, …, 18:00), with caching by city+date so repeated loads don’t change
- **Source labels**: Every row shows which provider supplied the data
- **CSV export**: Export the currently viewed dataset (including sources)
- **Modern UI**: Clean WPF interface

## Data Display

The application displays all required weather parameters:
- **Czas (Time)**: Current time of weather measurement
- **Miejscowość (Location)**: City and country information
- **Temperatura (Temperature)**: Temperature in Celsius
- **Wilgotność (Humidity)**: Relative humidity percentage
- **Ciśnienie (Pressure)**: Atmospheric pressure in hPa
- **Zachmurzenie (Cloudiness)**: Cloud cover percentage
- **Opady deszczu (Rainfall)**: Precipitation in mm

## API Integration

### OpenWeatherMap API
- Current weather: `https://api.openweathermap.org/data/2.5/weather`
- Geocoding: `http://api.openweathermap.org/geo/1.0/direct`
- Time Machine (past-day snapshots): `https://api.openweathermap.org/data/3.0/onecall/timemachine`
- Authentication: API key required (Time Machine requires an appropriate plan)

### IMGW API
- Current synoptic data: `https://danepubliczne.imgw.pl/api/data/synop`
- Station-by-name: `https://danepubliczne.imgw.pl/api/data/synop/station/{name}`
- Past by date (all stations): `https://danepubliczne.imgw.pl/api/data/synop/date/{yyyy-mm-dd}` (free, no key)
- Authentication: None required (public API)

## Setup

### 1. API Key Configuration

1. Get your OpenWeatherMap API key:
   - Visit [OpenWeatherMap API](https://openweathermap.org/api)
   - Sign up for a free account
   - Get your API key from the dashboard

2. Update the API key in `Configuration/AppConfig.cs`:
   ```csharp
   public const string OpenWeatherMapApiKey = "YOUR_ACTUAL_API_KEY_HERE";
   ```

### 2. Build and Run

1. Open the solution in Visual Studio
2. Restore NuGet packages
3. Build the solution
4. Run the application

## Usage

1. **Enter City Name**: Type a city name in the input field
2. **Get Weather Data**: Click either "Get OpenWeatherMap Data" or "Get IMGW Data"
3. **View Results**: Weather information will be displayed in the main panel
4. **View Dashboard**: Click "Show Weather Dashboard" to open charts and tools
5. **Pick a date**: Use the date picker to focus on a day
6. **Load past-day samples (OWM)**: Click "Load 10 samples" to fetch 10 fixed times for the chosen day (requires OWM Time Machine access). Results are cached per city+date so repeated loads don’t change
7. **Export Data**: Use the export button to save the current view as CSV

## Dashboard Features

- **Temperature/Humidity/Pressure/Rainfall views**: Tabular charts over time (include source)
- **Combined view**: All metrics in one table
- **Date filter**: Quickly filter to one day
- **OpenWeather past-day samples**: Deterministic 10-time snapshots for the selected day
- **CSV export**: Exports the filtered dataset with all metrics and `Source`

## Technical Details

- **Framework**: .NET 8.0 WPF
- **Dependencies**:
  - System.Text.Json (primary JSON handling)
  - Newtonsoft.Json (compatibility)
- **Architecture**: Simple service layer with typed models
- **Data Models**: `WeatherData` includes `Source` to indicate provider

## Project Structure

```
WeatherApp/
├── Models/
│   └── WeatherData.cs          # Data models for weather information
├── Services/
│   ├── IWeatherService.cs      # Weather service interface
│   ├── OpenWeatherMapService.cs # OpenWeatherMap API integration
│   └── ImgwService.cs          # IMGW API integration
├── Configuration/
│   └── AppConfig.cs            # Application configuration
├── MainWindow.xaml             # Main application window
├── MainWindow.xaml.cs          # Main window code-behind
├── WeatherDashboardWindow.xaml # Dashboard window
├── WeatherDashboardWindow.xaml.cs # Dashboard code-behind
└── README.md                   # This file
```

## Error Handling

The application includes comprehensive error handling:
- Network connectivity issues
- Invalid API responses
- Missing or invalid city names
- API rate limiting
- Data parsing errors

## Notes and Future Enhancements

- IMGW past-day loading in dashboard (via `api/data/synop/date/{yyyy-mm-dd}`)
- Weather alerts and notifications
- Multiple city comparison
- Advanced charting options
- Real-time data updates
- Weather forecast integration

If OpenWeather Time Machine is not available on your plan, the "Load 10 samples" action will indicate that no past data is available for the chosen day.

## License

This project is created for educational purposes as part of a desktop application development course.

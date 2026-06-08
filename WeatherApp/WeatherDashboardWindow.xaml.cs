using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WeatherApp.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using WeatherApp.Configuration;

namespace WeatherApp
{
    public partial class WeatherDashboardWindow : Window
    {
        private readonly List<WeatherData> _weatherData;
        private List<WeatherData> _filteredData;
        private readonly Dictionary<string, List<WeatherData>> _samplesCache = new Dictionary<string, List<WeatherData>>();

        public WeatherDashboardWindow(List<WeatherData> weatherData)
        {
            InitializeComponent();
            _weatherData = weatherData;
            _filteredData = new List<WeatherData>(_weatherData);
            StatusText.Text = $"Loaded {weatherData.Count} weather records";
        }

        private void TemperatureChartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Generating temperature chart...";
                
                var chart = GenerateTemperatureChart();
                DisplayChart(chart, "Temperature Analysis");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating temperature chart: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Error generating chart";
            }
        }

        private void HumidityChartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Generating humidity chart...";
                
                var chart = GenerateHumidityChart();
                DisplayChart(chart, "Humidity Analysis");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating humidity chart: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Error generating chart";
            }
        }

        private void PressureChartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Generating pressure chart...";
                
                var chart = GeneratePressureChart();
                DisplayChart(chart, "Pressure Analysis");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating pressure chart: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Error generating chart";
            }
        }

        private void RainfallChartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Generating rainfall chart...";
                
                var chart = GenerateRainfallChart();
                DisplayChart(chart, "Rainfall Analysis");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating rainfall chart: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Error generating chart";
            }
        }

        private void CombinedChartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Generating combined chart...";
                
                var chart = GenerateCombinedChart();
                DisplayChart(chart, "Combined Weather Analysis");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating combined chart: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Error generating chart";
            }
        }

        private void ExportDataButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var csv = GenerateCsvData();
                var fileName = $"weather_data_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = fileName,
                    DefaultExt = ".csv",
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, csv);
                    MessageBox.Show($"Data exported successfully to {saveFileDialog.FileName}", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                    StatusText.Text = $"Data exported to {fileName}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Error exporting data";
            }
        }

        

        

        

        

        

        

        

        private string GenerateCsvData()
        {
            var csv = new StringBuilder();
            csv.AppendLine("Time,Location,Temperature,Humidity,Pressure,Cloudiness,Rainfall,Description,Source");

            foreach (var data in _filteredData)
            {
                csv.AppendLine($"{data.Time:yyyy-MM-dd HH:mm},{data.Location},{data.Temperature:F1},{data.Humidity:F1},{data.Pressure:F1},{data.Cloudiness:F1},{data.Rainfall:F1},{data.Description},{data.Source}");
            }

            return csv.ToString();
        }

        private void ApplyDateFilter(DateTime? selectedDate)
        {
            if (selectedDate == null)
            {
                _filteredData = new List<WeatherData>(_weatherData);
            }
            else
            {
                var date = selectedDate.Value.Date;
                _filteredData = _weatherData
                    .Where(w => w.Time.Date == date)
                    .OrderBy(w => w.Time)
                    .ToList();
            }

            StatusText.Text = $"Filtered to {_filteredData.Count} records";
        }

        private void DateFilterPicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyDateFilter(DateFilterPicker.SelectedDate);
        }

        private List<WeatherData> DataForCharts => _filteredData ?? _weatherData;

        private string BuildSourceTitle()
        {
            var sources = DataForCharts.Select(d => d.Source).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
            if (sources.Count == 0) return string.Empty;
            return $"Data sources: {string.Join(", ", sources)}";
        }

        private StackPanel GenerateTemperatureChart()
        {
            var chartPanel = new StackPanel
            {
                Background = new SolidColorBrush(Color.FromRgb(250, 250, 250)),
                Margin = new Thickness(10)
            };

            var title = new TextBlock
            {
                Text = "Temperature Over Time",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };

            var dataGrid = new DataGrid
            {
                AutoGenerateColumns = false,
                CanUserAddRows = false,
                CanUserDeleteRows = false,
                IsReadOnly = true,
                GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
                HeadersVisibility = DataGridHeadersVisibility.Column,
                MaxHeight = 300
            };

            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Time", Binding = new System.Windows.Data.Binding("Time") { StringFormat = "yyyy-MM-dd HH:mm" }, Width = 150 });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Temperature (°C)", Binding = new System.Windows.Data.Binding("Temperature") { StringFormat = "F1" }, Width = 120 });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Source", Binding = new System.Windows.Data.Binding("Source"), Width = 120 });

            var tempData = DataForCharts.Select(w => new { w.Time, w.Temperature, w.Source }).OrderBy(x => x.Time).ToList();
            dataGrid.ItemsSource = tempData;

            chartPanel.Children.Add(title);
            chartPanel.Children.Add(dataGrid);

            return chartPanel;
        }

        private StackPanel GenerateHumidityChart()
        {
            var chartPanel = new StackPanel
            {
                Background = new SolidColorBrush(Color.FromRgb(250, 250, 250)),
                Margin = new Thickness(10)
            };

            var title = new TextBlock
            {
                Text = "Humidity Over Time",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };

            var dataGrid = new DataGrid
            {
                AutoGenerateColumns = false,
                CanUserAddRows = false,
                CanUserDeleteRows = false,
                IsReadOnly = true,
                GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
                HeadersVisibility = DataGridHeadersVisibility.Column,
                MaxHeight = 300
            };

            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Time", Binding = new System.Windows.Data.Binding("Time") { StringFormat = "yyyy-MM-dd HH:mm" }, Width = 150 });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Humidity (%)", Binding = new System.Windows.Data.Binding("Humidity") { StringFormat = "F1" }, Width = 120 });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Source", Binding = new System.Windows.Data.Binding("Source"), Width = 120 });

            var humidityData = DataForCharts.Select(w => new { w.Time, w.Humidity, w.Source }).OrderBy(x => x.Time).ToList();
            dataGrid.ItemsSource = humidityData;

            chartPanel.Children.Add(title);
            chartPanel.Children.Add(dataGrid);

            return chartPanel;
        }

        private StackPanel GeneratePressureChart()
        {
            var chartPanel = new StackPanel
            {
                Background = new SolidColorBrush(Color.FromRgb(250, 250, 250)),
                Margin = new Thickness(10)
            };

            var title = new TextBlock
            {
                Text = "Pressure Over Time",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };

            var dataGrid = new DataGrid
            {
                AutoGenerateColumns = false,
                CanUserAddRows = false,
                CanUserDeleteRows = false,
                IsReadOnly = true,
                GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
                HeadersVisibility = DataGridHeadersVisibility.Column,
                MaxHeight = 300
            };

            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Time", Binding = new System.Windows.Data.Binding("Time") { StringFormat = "yyyy-MM-dd HH:mm" }, Width = 150 });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Pressure (hPa)", Binding = new System.Windows.Data.Binding("Pressure") { StringFormat = "F1" }, Width = 120 });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Source", Binding = new System.Windows.Data.Binding("Source"), Width = 120 });

            var pressureData = DataForCharts.Select(w => new { w.Time, w.Pressure, w.Source }).OrderBy(x => x.Time).ToList();
            dataGrid.ItemsSource = pressureData;

            chartPanel.Children.Add(title);
            chartPanel.Children.Add(dataGrid);

            return chartPanel;
        }

        private StackPanel GenerateRainfallChart()
        {
            var chartPanel = new StackPanel
            {
                Background = new SolidColorBrush(Color.FromRgb(250, 250, 250)),
                Margin = new Thickness(10)
            };

            var title = new TextBlock
            {
                Text = "Rainfall Over Time",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };

            var dataGrid = new DataGrid
            {
                AutoGenerateColumns = false,
                CanUserAddRows = false,
                CanUserDeleteRows = false,
                IsReadOnly = true,
                GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
                HeadersVisibility = DataGridHeadersVisibility.Column,
                MaxHeight = 300
            };

            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Time", Binding = new System.Windows.Data.Binding("Time") { StringFormat = "yyyy-MM-dd HH:mm" }, Width = 150 });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Rainfall (mm)", Binding = new System.Windows.Data.Binding("Rainfall") { StringFormat = "F1" }, Width = 120 });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Source", Binding = new System.Windows.Data.Binding("Source"), Width = 120 });

            var rainfallData = DataForCharts.Select(w => new { w.Time, w.Rainfall, w.Source }).OrderBy(x => x.Time).ToList();
            dataGrid.ItemsSource = rainfallData;

            chartPanel.Children.Add(title);
            chartPanel.Children.Add(dataGrid);

            return chartPanel;
        }

        private StackPanel GenerateCombinedChart()
        {
            var chartPanel = new StackPanel
            {
                Background = new SolidColorBrush(Color.FromRgb(250, 250, 250)),
                Margin = new Thickness(10)
            };

            var title = new TextBlock
            {
                Text = "Combined Weather Analysis",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };

            var dataGrid = new DataGrid
            {
                AutoGenerateColumns = false,
                CanUserAddRows = false,
                CanUserDeleteRows = false,
                IsReadOnly = true,
                GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
                HeadersVisibility = DataGridHeadersVisibility.Column,
                MaxHeight = 400
            };

            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Time", Binding = new System.Windows.Data.Binding("Time") { StringFormat = "yyyy-MM-dd HH:mm" }, Width = 120 });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Temperature (°C)", Binding = new System.Windows.Data.Binding("Temperature") { StringFormat = "F1" }, Width = 100 });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Humidity (%)", Binding = new System.Windows.Data.Binding("Humidity") { StringFormat = "F1" }, Width = 100 });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Pressure (hPa)", Binding = new System.Windows.Data.Binding("Pressure") { StringFormat = "F1" }, Width = 100 });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Rainfall (mm)", Binding = new System.Windows.Data.Binding("Rainfall") { StringFormat = "F1" }, Width = 100 });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Source", Binding = new System.Windows.Data.Binding("Source"), Width = 120 });

            var combinedData = DataForCharts.OrderBy(x => x.Time).ToList();
            dataGrid.ItemsSource = combinedData;

            chartPanel.Children.Add(title);
            chartPanel.Children.Add(dataGrid);

            return chartPanel;
        }

        private async void GenerateSamplesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DateFilterPicker.SelectedDate == null)
                {
                    MessageBox.Show("Please pick a date first.", "Date required", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Decide location: prefer last loaded item location text (before comma country if present)
                var currentLocation = _weatherData.FirstOrDefault()?.Location ?? "Warsaw";
                var city = currentLocation.Split(',')[0].Trim();
                var day = DateFilterPicker.SelectedDate.Value.Date;

                var cacheKey = $"{city.ToLowerInvariant()}|{day:yyyy-MM-dd}";
                if (_samplesCache.TryGetValue(cacheKey, out var cached))
                {
                    _filteredData = cached.ToList();
                    StatusText.Text = $"Loaded cached OpenWeather data for {city} on {day:yyyy-MM-dd}";
                    return;
                }

                StatusText.Text = $"Fetching OpenWeather past data for {city} {day:yyyy-MM-dd}...";
                var samples = await FetchOpenWeatherDaySamplesAsync(city, day);
                if (samples.Count == 0)
                {
                    MessageBox.Show("No past data available from OpenWeather for that day (requires Time Machine access).", "No Data", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                _filteredData = samples;
                _samplesCache[cacheKey] = samples.ToList();
                StatusText.Text = $"Loaded {samples.Count} OpenWeather samples for {city} on {day:yyyy-MM-dd}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading OpenWeather past data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static readonly int[] s_sampleHours = new[] { 0, 2, 4, 6, 8, 10, 12, 14, 16, 18 };

        private async Task<List<WeatherData>> FetchOpenWeatherDaySamplesAsync(string city, DateTime date)
        {
            var results = new List<WeatherData>();
            using var http = new HttpClient();

            // 1) Geocode city to lat/lon
            var geoUrl = $"http://api.openweathermap.org/geo/1.0/direct?q={Uri.EscapeDataString(city)}&limit=1&appid={AppConfig.OpenWeatherMapApiKey}";
            var geoJson = await http.GetStringAsync(geoUrl);
            using var geoDoc = JsonDocument.Parse(geoJson);
            if (geoDoc.RootElement.GetArrayLength() == 0)
                return results;
            var lat = geoDoc.RootElement[0].GetProperty("lat").GetDouble();
            var lon = geoDoc.RootElement[0].GetProperty("lon").GetDouble();

            // 2) For each target hour, call Time Machine (requires appropriate OWM plan)
            foreach (var hour in s_sampleHours)
            {
                var dt = new DateTimeOffset(new DateTime(date.Year, date.Month, date.Day, hour, 0, 0, DateTimeKind.Utc)).ToUnixTimeSeconds();
                var url = $"https://api.openweathermap.org/data/3.0/onecall/timemachine?lat={lat.ToString(System.Globalization.CultureInfo.InvariantCulture)}&lon={lon.ToString(System.Globalization.CultureInfo.InvariantCulture)}&dt={dt}&appid={AppConfig.OpenWeatherMapApiKey}&units=metric";
                try
                {
                    var json = await http.GetStringAsync(url);
                    using var doc = JsonDocument.Parse(json);
                    // Response may include "data" (hourly snapshot) or "hourly" array. Try both.
                    JsonElement root = doc.RootElement;
                    JsonElement node;
                    if (root.TryGetProperty("data", out node))
                    {
                        var wd = MapOwmNodeToWeather(node, city);
                        if (wd != null) results.Add(wd);
                    }
                    else if (root.TryGetProperty("hourly", out node) && node.ValueKind == JsonValueKind.Array && node.GetArrayLength() > 0)
                    {
                        // find closest hour to our requested
                        WeatherData? best = null;
                        long target = dt;
                        foreach (var h in node.EnumerateArray())
                        {
                            var wd = MapOwmNodeToWeather(h, city);
                            if (wd == null) continue;
                            var diff = Math.Abs(new DateTimeOffset(wd.Time).ToUnixTimeSeconds() - target);
                            if (best == null || diff < Math.Abs(new DateTimeOffset(best.Time).ToUnixTimeSeconds() - target))
                                best = wd;
                        }
                        if (best != null) results.Add(best);
                    }
                }
                catch
                {
                    // skip this hour if not available
                }
            }

            return results
                .OrderBy(r => r.Time)
                .ToList();
        }

        private WeatherData? MapOwmNodeToWeather(JsonElement node, string city)
        {
            try
            {
                double temp = node.GetProperty("temp").GetDouble();
                double humidity = node.TryGetProperty("humidity", out var h) ? h.GetDouble() : 0;
                double pressure = node.TryGetProperty("pressure", out var p) ? p.GetDouble() : 0;
                double clouds = node.TryGetProperty("clouds", out var c) ? c.GetDouble() : 0;
                double rain = 0;
                if (node.TryGetProperty("rain", out var r))
                {
                    if (r.TryGetProperty("1h", out var r1h)) rain = r1h.GetDouble();
                }
                long dt = node.TryGetProperty("dt", out var dte) ? dte.GetInt64() : 0;
                var time = DateTimeOffset.FromUnixTimeSeconds(dt).UtcDateTime;

                return new WeatherData
                {
                    Time = time,
                    Location = city,
                    Temperature = temp,
                    Humidity = humidity,
                    Pressure = pressure,
                    Cloudiness = clouds,
                    Rainfall = rain,
                    Description = "OpenWeather TimeMachine",
                    Icon = "",
                    Source = "OpenWeatherMap"
                };
            }
            catch
            {
                return null;
            }
        }

        private void DisplayChart(StackPanel chart, string title)
        {
            ChartPanel.Children.Clear();

            var titleText = new TextBlock
            {
                Text = title,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20),
                Foreground = new SolidColorBrush(Color.FromRgb(33, 150, 243))
            };

            ChartPanel.Children.Add(titleText);
            ChartPanel.Children.Add(chart);

            var sourceText = new TextBlock
            {
                Text = BuildSourceTitle(),
                FontSize = 12,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117)),
                Margin = new Thickness(0, 10, 0, 0)
            };
            ChartPanel.Children.Add(sourceText);

            var summaryPanel = CreateDataSummary();
            ChartPanel.Children.Add(summaryPanel);

            StatusText.Text = $"Displayed {title} with {DataForCharts.Count} data points";
        }

        private StackPanel CreateDataSummary()
        {
            var summaryPanel = new StackPanel
            {
                Margin = new Thickness(0, 20, 0, 0),
                Background = new SolidColorBrush(Color.FromRgb(250, 250, 250))
            };

            var title = new TextBlock
            {
                Text = "Data Summary",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var avgTemp = DataForCharts.Any() ? DataForCharts.Average(w => w.Temperature) : 0;
            var avgHumidity = DataForCharts.Any() ? DataForCharts.Average(w => w.Humidity) : 0;
            var avgPressure = DataForCharts.Any() ? DataForCharts.Average(w => w.Pressure) : 0;
            var totalRainfall = DataForCharts.Any() ? DataForCharts.Sum(w => w.Rainfall) : 0;

            var summaryText = new TextBlock
            {
                Text = $"Average Temperature: {avgTemp:F1}°C\n" +
                      $"Average Humidity: {avgHumidity:F1}%\n" +
                      $"Average Pressure: {avgPressure:F1} hPa\n" +
                      $"Total Rainfall: {totalRainfall:F1} mm",
                FontSize = 14,
                LineHeight = 20
            };

            summaryPanel.Children.Add(title);
            summaryPanel.Children.Add(summaryText);

            return summaryPanel;
        }
    }
}

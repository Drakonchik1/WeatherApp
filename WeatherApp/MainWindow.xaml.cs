using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WeatherApp.Models;
using WeatherApp.Services;
using WeatherApp.Configuration;

namespace WeatherApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly HttpClient _httpClient;
        private readonly IWeatherService _openWeatherService;
        private readonly IWeatherService _imgwService;
        private readonly ObservableCollection<WeatherData> _weatherDataCollection;
        private readonly List<string> _cityList;

        public MainWindow()
        {
            InitializeComponent();
            
            _httpClient = new HttpClient();
            _openWeatherService = new OpenWeatherMapService(_httpClient, AppConfig.OpenWeatherMapApiKey);
            _imgwService = new ImgwService(_httpClient);
            _weatherDataCollection = new ObservableCollection<WeatherData>();
            
            WeatherDataGrid.ItemsSource = _weatherDataCollection;

            _cityList = BuildCityList();
            CityComboBox.ItemsSource = _cityList;
            CityComboBox.IsTextSearchCaseSensitive = false;
            CityComboBox.IsTextSearchEnabled = false; // we'll filter manually on TextChanged
            CityComboBox.AddHandler(System.Windows.Controls.Primitives.TextBoxBase.TextChangedEvent, new TextChangedEventHandler(CityComboBox_TextChanged));
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Load IMGW station names dynamically and merge into the list
                var response = await _httpClient.GetStringAsync("https://danepubliczne.imgw.pl/api/data/synop");
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var imgw = System.Text.Json.JsonSerializer.Deserialize<List<Models.ImgwResponse>>(response, options) ?? new List<Models.ImgwResponse>();

                var stationNames = imgw
                    .Select(s => s.stacja)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct(StringComparer.InvariantCultureIgnoreCase)
                    .ToList();

                var merged = _cityList
                    .Concat(stationNames)
                    .Distinct(StringComparer.InvariantCultureIgnoreCase)
                    .OrderBy(c => c, StringComparer.InvariantCultureIgnoreCase)
                    .ToList();

                CityComboBox.ItemsSource = merged;
            }
            catch
            {
                // Non-fatal; keep static list
            }
        }

        private async void OpenWeatherButton_Click(object sender, RoutedEventArgs e)
        {
            await GetWeatherData(_openWeatherService, "OpenWeatherMap");
        }

        private async void ImgwButton_Click(object sender, RoutedEventArgs e)
        {
            await GetWeatherData(_imgwService, "IMGW");
        }

        private async Task GetWeatherData(IWeatherService service, string sourceName)
        {
            try
            {
                StatusText.Text = $"Loading weather data from {sourceName}...";
                OpenWeatherButton.IsEnabled = false;
                ImgwButton.IsEnabled = false;

                var city = (CityComboBox.Text ?? string.Empty).Trim();
                if (string.IsNullOrEmpty(city))
                {
                    MessageBox.Show("Please enter a city name.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var weatherData = await service.GetCurrentWeatherAsync(city);
                if (weatherData != null)
                {
                    DisplayWeatherData(weatherData, sourceName);
                    _weatherDataCollection.Insert(0, weatherData);
                    StatusText.Text = $"Successfully loaded weather data from {sourceName}";
                }
                else
                {
                    string errorMessage = sourceName == "IMGW" 
                        ? $"Failed to load weather data from {sourceName}. Please try a Polish city name (e.g., Warsaw, Krakow, Gdansk)." 
                        : $"Failed to load weather data from {sourceName}. Please check the city name and try again.";
                    
                    MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusText.Text = $"Failed to load weather data from {sourceName}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Error occurred while loading weather data";
            }
            finally
            {
                OpenWeatherButton.IsEnabled = true;
                ImgwButton.IsEnabled = true;
            }
        }

        private void DisplayWeatherData(WeatherData weatherData, string sourceName)
        {
            WeatherDisplayPanel.Children.Clear();

            // Create weather display cards
            var mainCard = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(240, 248, 255)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(5),
                Margin = new Thickness(5),
                Padding = new Thickness(15)
            };

            var mainPanel = new StackPanel();
            
            // Header
            var headerPanel = new StackPanel();
            var sourceText = new TextBlock
            {
                Text = $"Data Source: {sourceName}",
                FontWeight = FontWeights.Bold,
                FontSize = 16,
                Foreground = new SolidColorBrush(Color.FromRgb(33, 150, 243))
            };
            var locationText = new TextBlock
            {
                Text = weatherData.Location,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 5, 0, 0)
            };
            var timeText = new TextBlock
            {
                Text = $"Updated: {weatherData.Time:yyyy-MM-dd HH:mm}",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                Margin = new Thickness(0, 2, 0, 10)
            };

            headerPanel.Children.Add(sourceText);
            headerPanel.Children.Add(locationText);
            headerPanel.Children.Add(timeText);

            // Weather details grid
            var detailsGrid = new Grid();
            detailsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            detailsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            detailsGrid.RowDefinitions.Add(new RowDefinition());
            detailsGrid.RowDefinitions.Add(new RowDefinition());
            detailsGrid.RowDefinitions.Add(new RowDefinition());

            // Temperature
            var tempPanel = CreateWeatherDetailPanel("🌡️ Temperature", $"{weatherData.Temperature:F1}°C", "Temperature");
            Grid.SetRow(tempPanel, 0);
            Grid.SetColumn(tempPanel, 0);
            detailsGrid.Children.Add(tempPanel);

            // Humidity
            var humidityPanel = CreateWeatherDetailPanel("💧 Humidity", $"{weatherData.Humidity:F1}%", "Humidity");
            Grid.SetRow(humidityPanel, 0);
            Grid.SetColumn(humidityPanel, 1);
            detailsGrid.Children.Add(humidityPanel);

            // Pressure
            var pressurePanel = CreateWeatherDetailPanel("🔽 Pressure", $"{weatherData.Pressure:F1} hPa", "Atmospheric Pressure");
            Grid.SetRow(pressurePanel, 1);
            Grid.SetColumn(pressurePanel, 0);
            detailsGrid.Children.Add(pressurePanel);

            // Cloudiness
            var cloudPanel = CreateWeatherDetailPanel("☁️ Cloudiness", $"{weatherData.Cloudiness:F1}%", "Cloud Cover");
            Grid.SetRow(cloudPanel, 1);
            Grid.SetColumn(cloudPanel, 1);
            detailsGrid.Children.Add(cloudPanel);

            // Rainfall
            var rainPanel = CreateWeatherDetailPanel("🌧️ Rainfall", $"{weatherData.Rainfall:F1} mm", "Precipitation");
            Grid.SetRow(rainPanel, 2);
            Grid.SetColumn(rainPanel, 0);
            detailsGrid.Children.Add(rainPanel);

            // Description
            var descPanel = CreateWeatherDetailPanel("📝 Description", weatherData.Description, "Weather Condition");
            Grid.SetRow(descPanel, 2);
            Grid.SetColumn(descPanel, 1);
            detailsGrid.Children.Add(descPanel);

            mainPanel.Children.Add(headerPanel);
            mainPanel.Children.Add(detailsGrid);
            mainCard.Child = mainPanel;
            WeatherDisplayPanel.Children.Add(mainCard);
        }

        private StackPanel CreateWeatherDetailPanel(string icon, string value, string description)
        {
            var panel = new StackPanel
            {
                Margin = new Thickness(5),
                Background = new SolidColorBrush(Color.FromRgb(250, 250, 250))
            };

            var iconText = new TextBlock
            {
                Text = icon,
                FontSize = 20,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var valueText = new TextBlock
            {
                Text = value,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 5, 0, 0)
            };

            var descText = new TextBlock
            {
                Text = description,
                FontSize = 10,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                Margin = new Thickness(0, 2, 0, 0)
            };

            panel.Children.Add(iconText);
            panel.Children.Add(valueText);
            panel.Children.Add(descText);

            return panel;
        }

        private void ShowDashboardButton_Click(object sender, RoutedEventArgs e)
        {
            if (_weatherDataCollection.Count == 0)
            {
                MessageBox.Show("No weather data available. Please load some weather data first.", 
                              "No Data", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dashboardWindow = new WeatherDashboardWindow(_weatherDataCollection.ToList());
            dashboardWindow.Show();
        }

        protected override void OnClosed(EventArgs e)
        {
            _httpClient?.Dispose();
            base.OnClosed(e);
        }

        private List<string> BuildCityList()
        {
            // Base set of international and Polish cities (you can extend this list or load from a file/API)
            var cities = new List<string>
            {
                // Poland
                "Warszawa","Kraków","Łódź","Wrocław","Poznań","Gdańsk","Szczecin","Bydgoszcz","Lublin","Białystok","Katowice","Gdynia","Częstochowa","Radom","Sosnowiec","Toruń","Kielce","Rzeszów","Gliwice","Zabrze",
                // English aliases
                "Warsaw","Cracow","Lodz","Wroclaw","Gdansk","Gdynia","Szczecin","Bialystok",
                // Europe sample
                "London","Paris","Berlin","Madrid","Rome","Vienna","Prague","Budapest","Vilnius","Riga","Tallinn","Oslo","Stockholm","Helsinki","Copenhagen","Amsterdam","Brussels","Zurich","Geneva","Lisbon",
                // World sample
                "New York","Los Angeles","Chicago","Toronto","Vancouver","Mexico City","São Paulo","Buenos Aires","Tokyo","Seoul","Sydney","Melbourne","Auckland","Cairo","Dubai"
            };

            return cities
                .Distinct(StringComparer.InvariantCultureIgnoreCase)
                .OrderBy(c => c, StringComparer.InvariantCultureIgnoreCase)
                .ToList();
        }

        private void CityComboBox_TextChanged(object? sender, TextChangedEventArgs e)
        {
            var text = (CityComboBox.Text ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(text))
            {
                CityComboBox.ItemsSource = _cityList;
                CityComboBox.IsDropDownOpen = true;
                return;
            }

            string Normalize(string s) => new string(s
                .Normalize(System.Text.NormalizationForm.FormD)
                .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                .ToArray())
                .ToLowerInvariant();

            var norm = Normalize(text);
            var filtered = _cityList
                .Where(c => Normalize(c).StartsWith(norm))
                .ToList();

            CityComboBox.ItemsSource = filtered;
            CityComboBox.IsDropDownOpen = true;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WeatherApp.Models;

namespace WeatherApp.Services
{
    public class ImgwService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://danepubliczne.imgw.pl/api/data/synop";

        public ImgwService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<WeatherData?> GetCurrentWeatherAsync(string location)
        {
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // 1) Try station-specific endpoint with several candidate spellings
                string[] BuildCandidates(string city)
                {
                    var raw = (city ?? string.Empty).Trim();
                    var lower = raw.ToLowerInvariant();
                    var candidates = new List<string>();

                    string alias = lower switch
                    {
                        "warsaw" => "warszawa",
                        "cracow" => "kraków",
                        "krakow" => "kraków",
                        "lodz" => "łódź",
                        "wroclaw" => "wrocław",
                        _ => lower
                    };

                    // variations
                    candidates.Add(raw);
                    candidates.Add(alias);
                    if (!string.IsNullOrWhiteSpace(alias))
                    {
                        var cap = char.ToUpperInvariant(alias[0]) + (alias.Length > 1 ? alias.Substring(1) : string.Empty);
                        candidates.Add(cap);
                    }
                    candidates.Add(lower);
                    return candidates.Distinct(StringComparer.InvariantCultureIgnoreCase).ToArray();
                }

                async Task<ImgwResponse?> TryStationAsync(string city)
                {
                    foreach (var cand in BuildCandidates(city))
                    {
                        try
                        {
                            if (string.IsNullOrWhiteSpace(cand)) continue;
                            var url = $"{BaseUrl}/station/{Uri.EscapeDataString(cand)}";
                            var json = await _httpClient.GetStringAsync(url);
                            if (string.IsNullOrWhiteSpace(json)) continue;
                            var trimmed = json.Trim();
                            if (trimmed == "[]") continue;
                            if (trimmed.StartsWith("["))
                            {
                                var arr = JsonSerializer.Deserialize<ImgwResponse[]>(trimmed, options);
                                var item = arr?.FirstOrDefault();
                                if (item != null) return item;
                            }
                            else
                            {
                                var item = JsonSerializer.Deserialize<ImgwResponse>(trimmed, options);
                                if (item != null) return item;
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"IMGW /station miss '{cand}': {ex.Message}");
                        }
                    }
                    return null;
                }

                ImgwResponse? station = await TryStationAsync(location);

                // 2) Fallback: download all stations and match
                if (station == null)
                {
                    var response = await _httpClient.GetStringAsync(BaseUrl);
                    var imgwData = JsonSerializer.Deserialize<ImgwResponse[]>(response, options);
                    if (imgwData == null || !imgwData.Any())
                    {
                        System.Diagnostics.Debug.WriteLine("No IMGW data received");
                        return null;
                    }

                    string Normalize(string text)
                    {
                        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
                        var lettersOnly = new string(text
                            .Where(ch => char.IsLetter(ch) || char.IsWhiteSpace(ch))
                            .ToArray());
                        return string
                            .Concat(lettersOnly.Normalize(System.Text.NormalizationForm.FormD)
                            .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark))
                            .ToLowerInvariant()
                            .Trim();
                    }

                    var normQuery = Normalize(location.Trim());
                    var alias = normQuery switch
                    {
                        "warsaw" => "warszawa",
                        "cracow" => "krakow",
                        "lodz" => "lodz",
                        "wroclaw" => "wroclaw",
                        "gdansk" => "gdansk",
                        _ => normQuery
                    };

                    station = imgwData.FirstOrDefault(s => Normalize(s.stacja) == alias)
                           ?? imgwData.FirstOrDefault(s => Normalize(s.stacja).StartsWith(alias))
                           ?? imgwData.FirstOrDefault(s => Normalize(s.stacja).Contains(alias));
                    if (station == null && alias == "warszawa")
                    {
                        station = imgwData.FirstOrDefault(s => Normalize(s.stacja) == "warszawa");
                    }
                    station ??= imgwData.FirstOrDefault();
                }

                if (station == null) 
                {
                    System.Diagnostics.Debug.WriteLine("No matching station found");
                    return null;
                }

                // Parse the date and time properly
                DateTime dateTime;
                int parsedHour = 0;
                if (!string.IsNullOrWhiteSpace(station.godzina_pomiaru))
                {
                    var hourText = station.godzina_pomiaru.Trim();
                    // examples: "0", "00", "12", "23"
                    if (!int.TryParse(hourText, out parsedHour))
                    {
                        parsedHour = 0;
                    }
                }
                if (!DateTime.TryParse($"{station.data_pomiaru} {parsedHour:D2}:00", out dateTime))
                {
                    // Fallback: just use today's date with provided hour
                    var today = DateTime.Today;
                    var hour = Math.Max(0, Math.Min(23, parsedHour));
                    dateTime = new DateTime(today.Year, today.Month, today.Day, hour, 0, 0);
                }

                // Safe numeric parsing (fields may be "" in API)
                static double ParseOrZero(string? s)
                {
                    if (string.IsNullOrWhiteSpace(s)) return 0;
                    s = s.Replace(",", ".");
                    if (double.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v))
                        return v;
                    return 0;
                }

                return new WeatherData
                {
                    Time = dateTime,
                    Location = station.stacja,
                    Temperature = ParseOrZero(station.temperatura),
                    Humidity = ParseOrZero(station.wilgotnosc_wzgledna),
                    Pressure = ParseOrZero(station.cisnienie),
                    Cloudiness = ParseOrZero(station.zachmurzenie_ogolne),
                    Rainfall = ParseOrZero(station.suma_opadu),
                    Description = "IMGW Data",
                    Icon = "imgw",
                    Source = "IMGW"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching weather from IMGW: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        public async Task<List<WeatherData>> GetHistoricalWeatherAsync(string location, int days = 7)
        {
            try
            {
                // IMGW doesn't provide historical data in the same format
                // For demo purposes, we'll generate sample data
                var weatherData = new List<WeatherData>();
                var random = new Random();
                
                for (int i = 0; i < days; i++)
                {
                    var date = DateTime.Now.AddDays(-i);
                    weatherData.Add(new WeatherData
                    {
                        Time = date,
                        Location = location,
                        Temperature = 10 + random.NextDouble() * 20, // 10-30°C
                        Humidity = 30 + random.NextDouble() * 50, // 30-80%
                        Pressure = 990 + random.NextDouble() * 60, // 990-1050 hPa
                        Cloudiness = random.NextDouble() * 100, // 0-100%
                        Rainfall = random.NextDouble() * 15, // 0-15mm
                        Description = "IMGW Historical",
                        Icon = "imgw"
                    });
                }

                return weatherData;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching historical weather from IMGW: {ex.Message}");
                return new List<WeatherData>();
            }
        }
    }
}
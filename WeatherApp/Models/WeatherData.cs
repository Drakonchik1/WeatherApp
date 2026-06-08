using System;
using System.Text.Json.Serialization;

namespace WeatherApp.Models
{
    public class WeatherData
    {
        public DateTime Time { get; set; }
        public string Location { get; set; } = string.Empty;
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double Pressure { get; set; }
        public double Cloudiness { get; set; }
        public double Rainfall { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
    }

    // OpenWeatherMap API Models
    public class OpenWeatherMapResponse
    {
        public string Name { get; set; } = string.Empty;
        public long Dt { get; set; }
        public Main Main { get; set; } = new();
        public Weather[] Weather { get; set; } = Array.Empty<Weather>();
        public Clouds Clouds { get; set; } = new();
        public Rain? Rain { get; set; }
        public Sys Sys { get; set; } = new();
    }

    public class Main
    {
        public double Temp { get; set; }
        public double Humidity { get; set; }
        public double Pressure { get; set; }
    }

    public class Weather
    {
        public string Main { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }

    public class Clouds
    {
        public double All { get; set; }
    }

    public class Rain
    {
        [System.Text.Json.Serialization.JsonPropertyName("1h")]
        public double OneHour { get; set; }
    }

    public class Sys
    {
        public string Country { get; set; } = string.Empty;
    }

    // IMGW API Models
    public class ImgwResponse
    {
        public string id_stacji { get; set; } = string.Empty;
        public string stacja { get; set; } = string.Empty;
        public string data_pomiaru { get; set; } = string.Empty;
        public string godzina_pomiaru { get; set; } = string.Empty;
        public string? temperatura { get; set; }
        public string? predkosc_wiatru { get; set; }
        public string? kierunek_wiatru { get; set; }
        public string? wilgotnosc_wzgledna { get; set; }
        public string? suma_opadu { get; set; }
        public string? cisnienie { get; set; }
        public string? zachmurzenie_ogolne { get; set; }
    }
}

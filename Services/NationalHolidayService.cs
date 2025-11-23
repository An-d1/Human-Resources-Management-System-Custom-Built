using HRManagementSystem.DTO;
using System.Text.Json;

public class NationalHolidayService
{
    private readonly HttpClient _http;

    public NationalHolidayService(HttpClient http)
    {   
        _http = http;
    }

    public async Task<List<NagerHolidayDto>> GetHolidaysAsync(int year, string countryCode)
    {
        var url = $"https://date.nager.at/api/v3/PublicHolidays/{year}/{countryCode}";

        var response = await _http.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<NagerHolidayDto>>(json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // As it has CamelCase
            });
    }
}


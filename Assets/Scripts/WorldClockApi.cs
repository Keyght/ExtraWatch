using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

public static class WorldClockApi
{
    public static async Task<DayData> WorldClockApiGet()
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://world-clock.p.rapidapi.com/json/utc/now"),
            Headers =
            {
                { "X-RapidAPI-Key", "21527b769cmsh629c37f9705c479p17c42ejsn3fe796da7329" },
                { "X-RapidAPI-Host", "world-clock.p.rapidapi.com" },
            },
        };
        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        await Task.Yield();
        var headers = response.Headers.Date.Value;
        var dayData = new DayData(headers.Hour, headers.Minute, headers.Second);
        Debug.Log("WorldClockApi returns time: " + dayData);
        return dayData;
    }
}
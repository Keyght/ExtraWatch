using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

public class Watch : MonoBehaviour
{
    [SerializeField] private Transform _hour;
    [SerializeField] private Transform _minute;
    [SerializeField] private Transform _second;

    private int _gmtOffset;
    
    
    private void Start()
    {
        var nowTime = DateTime.Now;
        SetClockTo(new DayData(nowTime.Hour, nowTime.Minute, nowTime.Second));
        _gmtOffset = TimeZoneInfo.Local.BaseUtcOffset.Hours;
        RapidApiGet();
    }

    private async Task RapidApiGet()
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
        if (response.Headers.Date != null)
        {
            var headers = response.Headers.Date.Value;
            var dayTime = new DayData(headers.Hour, headers.Minute, headers.Second);
            SetClockTo(dayTime);
            Debug.Log("RapidApi sets time to: " + dayTime);
        }
    }

    private void SetClockTo(DayData dayData)
    {
        ResetClocks();
        var secondsInMinute = dayData.Second / 60f;
        _second.Rotate(0, 0, secondsInMinute * 360);
        var minutesInHour = (dayData.Minute + secondsInMinute) / 60f ;
        _minute.Rotate(0, 0, minutesInHour * 360);
        var hoursInDay = (dayData.Hour + minutesInHour) / 12f;        
        _hour.Rotate(0, 0, hoursInDay * 360);
    }

    private void ResetClocks()
    {
        var firstPos = Quaternion.Euler(-90, 0, 0);
        _hour.localRotation = firstPos;
        _minute.localRotation = firstPos;
        _second.localRotation = firstPos;
    }
}

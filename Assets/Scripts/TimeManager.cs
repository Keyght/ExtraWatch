using System;
using System.Net.Http;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class TimeManager : MonoBehaviour
{
    [SerializeField] private Transform _hourArrow;
    [SerializeField] private Transform _minuteArrow;
    [SerializeField] private Transform _secondArrow;
    [SerializeField] private TextMeshProUGUI _hour;
    [SerializeField] private TextMeshProUGUI _minute;
    [SerializeField] private TextMeshProUGUI _second;
    
    private int _gmtOffset;

    public DayData CurrentTime => new DayData(int.Parse(_hour.text), int.Parse(_minute.text), int.Parse(_second.text));


    private void Start()
    {
        var nowTime = DateTime.Now;
        SetClockTo(new DayData(nowTime.Hour, nowTime.Minute, nowTime.Second));
        _gmtOffset = TimeZoneInfo.Local.BaseUtcOffset.Hours;
        TimeApi();
        WorldClockApi();
    }

    private async Task TimeApi()
    {
        using var webRequest = UnityWebRequest.Get("https://timeapi.io/api/Time/current/zone?timeZone=Europe/London");
        webRequest.SendWebRequest();
        while (!webRequest.isDone)
        {
            await Task.Yield();
        }

        var dayData = JsonUtility.FromJson<DayData>(webRequest.downloadHandler.text);
        dayData.hour = (dayData.hour + _gmtOffset) % 24;
        SetClockTo(dayData);
        Debug.Log("TimeApi sets time to: " + dayData);
    }

    private async Task WorldClockApi()
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
            var dayData = new DayData((headers.Hour + _gmtOffset) % 24, headers.Minute, headers.Second);
            //if (dayTime.Equals(CurrentTime))
            SetClockTo(dayData);
            Debug.Log("WorldClockApi sets time to: " + dayData);
        }
    }

    private void SetClockTo(DayData dayData)
    {
        ResetClocks();
        var secondsInMinute = dayData.seconds / 60f;
        _secondArrow.Rotate(0, 0, secondsInMinute * 360);
        var minutesInHour = (dayData.minute + secondsInMinute) / 60f ;
        _minuteArrow.Rotate(0, 0, minutesInHour * 360);
        var hoursInDay = (dayData.hour + minutesInHour) / 12f;        
        _hourArrow.Rotate(0, 0, hoursInDay * 360);
    }

    private void ResetClocks()
    {
        var firstPos = Quaternion.Euler(-90, 0, 0);
        _hourArrow.localRotation = firstPos;
        _minuteArrow.localRotation = firstPos;
        _secondArrow.localRotation = firstPos;
    }
}

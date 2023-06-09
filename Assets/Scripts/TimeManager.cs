using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [SerializeField] private Transform _hourArrow;
    [SerializeField] private Transform _minuteArrow;
    [SerializeField] private Transform _secondArrow;
    [SerializeField] private TextMeshProUGUI _hour;
    [SerializeField] private TextMeshProUGUI _minute;
    [SerializeField] private TextMeshProUGUI _second;
    [SerializeField] private float _apiRequestTimeInHours;
    [SerializeField] private float _timeScale;

    private int _gmtOffset;

    private CancellationTokenSource _cts;
    private static TimeManager _instance;

    public DayData CurrentTime => new(int.Parse(_hour.text), int.Parse(_minute.text), int.Parse(_second.text));
    public CancellationTokenSource CTS => _cts;
    public static TimeManager Instance => _instance;

    private void Awake()
    {
        _instance = this;
        _cts = new CancellationTokenSource();
    }

    private void Start()
    {
        _gmtOffset = TimeZoneInfo.Local.BaseUtcOffset.Hours;
        var time = DateTime.Now;
        SetClockTo(new DayData(time.Hour, time.Minute, time.Second));
        Time.timeScale = _timeScale;
        TickTime(_cts.Token);
        CorrectTime(_cts.Token);
    }

    private async Task TickTime(CancellationToken token)
    {
        var time = 1f;
        const float secondOnEuler = 360 / 60f;
        var secChange = false;
        var minChange = false;
        while (!token.IsCancellationRequested)
        {
            time -= Time.deltaTime;
            if (time <= 0)
            {
                _secondArrow.Rotate(0, 0, secondOnEuler);
                _second.SetText(((int.Parse(_second.text) + 1) % 60).ToString());
                time += 1f;
                secChange = true;
            }

            if (secChange && Math.Abs(_secondArrow.localRotation.z) < 0.01)
            {
                _minute.SetText(((int.Parse(_minute.text) + 1) % 60).ToString());
                RotateArrows(new Vector3(0, 0, secondOnEuler), _minuteArrow, _hourArrow);
                minChange = true;
                secChange = false;
            }
            if (minChange && int.Parse(_minute.text) == 0)
            {
                _hour.SetText(((int.Parse(_hour.text) + 1) % 24).ToString());
                minChange = false;
            }

            await Task.Yield();
        }
    }

    public static void RotateArrows(Vector3 rotationVector, Transform minuteArrow, Transform hourArrow)
    {
        minuteArrow.Rotate(rotationVector);
        hourArrow.Rotate(rotationVector / 12f);
    }

    private async Task CorrectTime(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var dayData = await TimeApi.TimeApiGet();
            dayData.hour = (dayData.hour + _gmtOffset) % 24;
            if (!dayData.Equals(CurrentTime))
                SetClockTo(dayData);

            dayData = await WorldClockApi.WorldClockApiGet();
            dayData.hour = (dayData.hour + _gmtOffset) % 24;
            if (!dayData.Equals(CurrentTime))
                SetClockTo(dayData);

            var hours = _apiRequestTimeInHours * 60 * 60;
            while (hours > 0)
            {
                hours -= Time.deltaTime;
                await Task.Yield();
            }
        }
    }

    private void SetClockTo(DayData dayData)
    {
        _hour.SetText(dayData.hour.ToString());
        _minute.SetText(dayData.minute.ToString());
        _second.SetText(dayData.seconds.ToString());

        ResetArrows();
        var secondsInMinute = dayData.seconds / 60f;
        _secondArrow.Rotate(0, 0, secondsInMinute * 360);
        var minutesInHour = dayData.minute / 60f;
        _minuteArrow.Rotate(0, 0, minutesInHour * 360);
        var hoursInDay = (dayData.hour + minutesInHour) / 12f;
        _hourArrow.Rotate(0, 0, hoursInDay * 360);
    }

    private void ResetArrows()
    {
        var firstPos = Quaternion.Euler(-90, 0, 0);
        _hourArrow.localRotation = firstPos;
        _minuteArrow.localRotation = firstPos;
        _secondArrow.localRotation = firstPos;
    }

    private void OnDestroy()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
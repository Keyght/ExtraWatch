using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Alarm : MonoBehaviour
{
    [SerializeField] private Transform _watch;
    [SerializeField] private Transform _watchForAlarm;

    [SerializeField] private TMP_Dropdown _dropdown;
    
    [SerializeField] private Button _clockButton;
    [SerializeField] private Button _watchButton;
    
    [SerializeField] private TMP_InputField _hourInput;
    [SerializeField] private TMP_InputField _minuteInput;
    [SerializeField] private TMP_InputField _secondInput;

    [SerializeField] private Button _plusButton;
    [SerializeField] private Button _minusButton;

    [SerializeField] private Button _cancelClock;
    [SerializeField] private Button _cancelWatch;
    
    [SerializeField] private GameObject _alarmUI;

    private bool _poinerPressed;
    private CancellationTokenSource _cancelAlarmSource;

    private void Awake()
    {
        _cancelAlarmSource = new CancellationTokenSource();
    }

    private void Start()
    {
        _clockButton.onClick.AddListener(OnClockButtonPressed);
        _watchButton.onClick.AddListener(OnWatchButtonPressed);
        
        _cancelClock.onClick.AddListener( CancelAlarm);
        _cancelWatch.onClick.AddListener(CancelAlarm);
        
        _plusButton.onClick.AddListener(delegate { RotatePoiner(true);});
        _minusButton.onClick.AddListener(delegate { RotatePoiner(false); });
    }

    private void RotatePoiner(bool sign)
    {
        if (_clockButton.interactable)
        {
            _watch.gameObject.SetActive(false);
            _watchForAlarm.gameObject.SetActive(true);
            for (var i = 0; i < _watch.childCount; i++)
            {
                _watchForAlarm.GetChild(i).rotation = _watch.GetChild(i).rotation;
            }
        }
        _clockButton.interactable = false;

        var rotationVector = sign ? new Vector3(0, 0, 6f) : new Vector3(0, 0, -6f);
        TimeManager.RotateArrows(rotationVector, _watchForAlarm.GetChild(3), _watchForAlarm.GetChild(2));
    }

    private async void OnWatchButtonPressed()
    {
        SetInteractions(false);
        await WatchAlarm();
        SetInteractions(true);
    }

    private async Task WatchAlarm()
    {
        var alarmData = TimeManager.Instance.CurrentTime;
        alarmData.hour = (int)(_watchForAlarm.GetChild(2).rotation.eulerAngles.z * 6 / 180f);
        if (_dropdown.value == 0) alarmData.hour += 12;
        alarmData.minute = (int)(_watchForAlarm.GetChild(3).rotation.eulerAngles.z * 30 / 180f);
        alarmData.seconds = (int)(_watchForAlarm.GetChild(4).rotation.eulerAngles.z * 30 / 180f);
        await Alarming(alarmData);
        _watch.gameObject.SetActive(true);
        _watchForAlarm.gameObject.SetActive(false);
    }

    private async void OnClockButtonPressed()
    {
        SetInteractions(false);
        await ClockAlarm();
        SetInteractions(true);
    }

    private async Task ClockAlarm()
    {
        var alarmData = TimeManager.Instance.CurrentTime;
        alarmData.hour = string.IsNullOrWhiteSpace(_hourInput.text) ? alarmData.hour : int.Parse(_hourInput.text);
        alarmData.minute = string.IsNullOrWhiteSpace(_minuteInput.text) ? alarmData.minute : int.Parse(_minuteInput.text) ;
        alarmData.seconds = string.IsNullOrWhiteSpace(_secondInput.text) ? alarmData.seconds : int.Parse(_secondInput.text);
        _hourInput.text = alarmData.hour.ToString();
        _minuteInput.text = alarmData.minute.ToString();
        _secondInput.text = alarmData.seconds.ToString();
        await Alarming(alarmData);
        _hourInput.text = "";
        _minuteInput.text = "";
        _secondInput.text = "";
    }

    private async Task Alarming(DayData alarmData)
    {
        Debug.Log("Alarm sets to: " + alarmData);
        while (!alarmData.Equals(TimeManager.Instance.CurrentTime) && !_cancelAlarmSource.Token.IsCancellationRequested)
        {
            await Task.Yield();
        }
        if (!_cancelAlarmSource.Token.IsCancellationRequested)
        {
            _alarmUI.SetActive(true);
            StartCoroutine(ShowForSeconds(5, _alarmUI));
        }
    }
    
    private async void CancelAlarm()
    {
        _cancelAlarmSource.Cancel();
        await Task.Delay(1000);
        _cancelAlarmSource.Dispose();
        _cancelAlarmSource = new CancellationTokenSource();
    }

    private void SetInteractions(bool flag)
    {
        _plusButton.interactable = flag;
        _minusButton.interactable = flag;
        _watchButton.interactable = flag;
        _clockButton.interactable = flag;
    }

    private static IEnumerator ShowForSeconds(float sec, GameObject toShow)
    {
        yield return new WaitForSeconds(sec);
        
        toShow.SetActive(false);
    }

    private void OnDestroy()
    {
        _cancelAlarmSource.Cancel();
        _cancelAlarmSource.Dispose();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Alarm : MonoBehaviour
{
    [SerializeField] private Transform _watch;
    [SerializeField] private Transform _watchForAlarm;
    
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
    private CancellationTokenSource _cancelClockSource;
    private CancellationTokenSource _cancelWatchSource;

    private void Awake()
    {
        _cancelClockSource = new CancellationTokenSource();
        _cancelWatchSource = new CancellationTokenSource();
    }

    private void Start()
    {
        _clockButton.onClick.AddListener(OnClockButtonPressed);
        _watchButton.onClick.AddListener(OnWatchButtonPressed);
        
        _cancelClock.onClick.AddListener(delegate { CancelAlarm(_cancelClockSource);});
        _cancelWatch.onClick.AddListener(delegate { CancelAlarm(_cancelWatchSource);});
        
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

    private void OnWatchButtonPressed()
    {
        SetInteractions(false);
        
        SetInteractions(true);
    }

    private async void OnClockButtonPressed()
    {
        SetInteractions(false);
        await ClockAlarm();
        SetInteractions(true);
    }

    private async Task ClockAlarm()
    {
        var timeManager = TimeManager.Instance;
        var alarmData = timeManager.CurrentTime;
        alarmData.hour = string.IsNullOrWhiteSpace(_hourInput.text) ? alarmData.hour : int.Parse(_hourInput.text);
        alarmData.minute = string.IsNullOrWhiteSpace(_minuteInput.text) ? alarmData.minute : int.Parse(_minuteInput.text) ;
        alarmData.seconds = string.IsNullOrWhiteSpace(_secondInput.text) ? alarmData.seconds : int.Parse(_secondInput.text);
        Debug.Log("Alarm sets to: " + alarmData);
        _hourInput.text = alarmData.hour.ToString();
        _minuteInput.text = alarmData.minute.ToString();
        _secondInput.text = alarmData.seconds.ToString();
        while (!alarmData.Equals(timeManager.CurrentTime) && !_cancelClockSource.Token.IsCancellationRequested)
        {
            await Task.Yield();
        }
        if (!_cancelClockSource.Token.IsCancellationRequested)
        {
            _alarmUI.SetActive(true);
            StartCoroutine(ShowForSeconds(5, _alarmUI));
        }
        _hourInput.text = "";
        _minuteInput.text = "";
        _secondInput.text = "";
    }
    
    private async void CancelAlarm(CancellationTokenSource source)
    {
        source.Cancel();
        await Task.Delay(1500);
        if (source.Equals(_cancelClockSource)) _cancelClockSource = new CancellationTokenSource();
        else _cancelWatchSource = new CancellationTokenSource();
        source.Dispose();
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
        _cancelClockSource.Cancel();
        _cancelWatchSource.Cancel();
        _cancelClockSource.Dispose();
        _cancelWatchSource.Dispose();
    }
}

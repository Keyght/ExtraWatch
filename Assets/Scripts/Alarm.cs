using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Alarm : MonoBehaviour
{
    [SerializeField] private Button _clockButton;
    [SerializeField] private Button _watchButton;
    
    [SerializeField] private TMP_InputField _hourInput;
    [SerializeField] private TMP_InputField _minuteInput;
    [SerializeField] private TMP_InputField _secondInput;

    [SerializeField] private Button _plusButton;
    [SerializeField] private Button _minusButton;
    
    [SerializeField] private GameObject _alarmUI; 

    private void Start()
    {
        _clockButton.onClick.AddListener(OnClockButtonPressed);
        _watchButton.onClick.AddListener(OnWatchButtonPressed);
    }

    private void OnWatchButtonPressed()
    {
        _watchButton.interactable = false;
        _clockButton.interactable = false;
    }

    private async void OnClockButtonPressed()
    {
        _watchButton.interactable = false;
        _clockButton.interactable = false;
        await ClockAlarm();
        _watchButton.interactable = true;
        _clockButton.interactable = true;
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
        while (!alarmData.Equals(timeManager.CurrentTime) && !timeManager.CTS.Token.IsCancellationRequested)
        {
            await Task.Yield();
        }
        _alarmUI.SetActive(true);
        StartCoroutine(ShowForSeconds(5, _alarmUI));
        _hourInput.text = "";
        _minuteInput.text = "";
        _secondInput.text = "";
    }

    private static IEnumerator ShowForSeconds(float sec, GameObject toShow)
    {
        yield return new WaitForSeconds(sec);
        
        toShow.SetActive(false);
    }
}

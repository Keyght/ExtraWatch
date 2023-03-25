using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class ArrowRotation : MonoBehaviour
{
    [SerializeField] private Transform _hour;
    [SerializeField] private Transform _minute;
    [SerializeField] private Transform _second;

    private CancellationTokenSource _cts;

    private void Awake()
    {
        _cts = new CancellationTokenSource();
    }

    private void Start()
    {
        RotateByTime(_cts.Token);
    }

    private async Task RotateByTime(CancellationToken token)
    {
        var time = 1f;
        var secondOnEuler = 360 / 60f;
        while (!token.IsCancellationRequested)
        {
            time -= Time.deltaTime;
            if (time <= 0)
            {
                _second.Rotate(0,0, secondOnEuler);
                time += 1f;
            }
            if (Math.Abs(time - 1f) < 0.01 && Math.Abs(_second.localRotation.z) < 0.01)
            {
                _minute.Rotate(0, 0, secondOnEuler);
                _hour.Rotate(0, 0, secondOnEuler / 12f);
            }
            await Task.Yield();
        }
    }

    private void OnDestroy()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}

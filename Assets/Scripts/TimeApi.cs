using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class TimeApi
{
    public static async Task<DayData> TimeApiGet()
    {
        using var webRequest = UnityWebRequest.Get("https://timeapi.io/api/Time/current/zone?timeZone=utc");
        webRequest.SendWebRequest();
        while (!webRequest.isDone)
        {
            await Task.Yield();
        }

        var dayData = JsonUtility.FromJson<DayData>(webRequest.downloadHandler.text);
        Debug.Log("TimeApi returns time: " + dayData);
        return dayData;
    }
}
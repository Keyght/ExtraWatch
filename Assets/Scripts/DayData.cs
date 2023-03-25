using System;

public struct DayData
{
    public int Hour;
    public int Minute;
    public int Second;

    public DayData(int hour, int minute, int second)
    {
        Hour = hour;
        Minute = minute;
        Second = second;
    }

    public override string ToString()
    {
        return "[Hours: " + Hour + " Minutes: " + Minute + " Seconds: " + Second;
    }
}
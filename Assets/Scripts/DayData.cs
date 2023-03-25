using System;

public struct DayData : IEquatable<DayData>
{
    public int hour;
    public int minute;
    public int seconds;

    public DayData(int hour, int minute, int seconds)
    {
        this.hour = hour;
        this.minute = minute;
        this.seconds = seconds;
    }

    public override string ToString()
    {
        return "Hours: " + hour + " Minutes: " + minute + " Seconds: " + seconds;
    }

    public bool Equals(DayData other)
    {
        return hour == other.hour && minute == other.minute && seconds == other.seconds;
    }

    public override bool Equals(object obj)
    {
        return obj is DayData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(hour, minute, seconds);
    }
}
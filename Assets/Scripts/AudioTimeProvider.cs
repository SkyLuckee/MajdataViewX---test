using System;
using UnityEngine;
// using System.Collections.Generic;
// using System.Linq;
// using MajSimai;

public class AudioTimeProvider : MonoBehaviour
{
    public float AudioTime; //notes get this value
    public bool isStart;
    public bool isRecord;
    public float offset;
    private float speed;

    private float startTime;
    private long ticks;

    public float CurrentSpeed => isRecord ? Time.timeScale : speed;

    // Update is called once per frame
    private void Update()
    {
        if (isStart)
        {
            if (isRecord)
                AudioTime = Time.time - startTime + offset;
            else
                AudioTime = (Time.realtimeSinceStartup - startTime) * speed + offset;
        }
    }
    public float GetFrame()
    {
        var _audioTime = AudioTime * 1000;

        return _audioTime / 16.6667f;
    }

    // optional field to keep the extracted BPM if you want it later
    // public float FirstBpm { get; private set; } = -1f;

    // Old 3-argument call compatibility
    // public void SetStartTime(long _ticks, float _offset, float _speed)
    // {
    //     SetStartTime(_ticks, _offset, _speed, (IEnumerable<SimaiTimingPoint>)null, false);
    // }

    // Old 4-argument call where the 4th argument is the record flag
    // public void SetStartTime(long _ticks, float _offset, float _speed, bool _isRecord)
    // {
    //     SetStartTime(_ticks, _offset, _speed, (IEnumerable<SimaiTimingPoint>)null, _isRecord);
    // }

    public void SetStartTime(long _ticks, float _offset, float _speed, bool _isRecord = false) //IEnumerable<SimaiTimingPoint> timings
    {
        // float firstBpm = timings?.FirstOrDefault()?.Bpm ?? -1f;
        ticks = _ticks;
        offset = _offset;
        AudioTime = offset;
        var dateTime = new DateTime(ticks);
        var seconds = (dateTime - DateTime.Now).TotalSeconds;
        isRecord = _isRecord;
        if (_isRecord)
        {
            startTime = Time.time + 5; //+ 60*4 / firstBpm;
            Time.timeScale = _speed;
            Time.captureFramerate = 60;
        }
        else
        {
            startTime = Time.realtimeSinceStartup + (float)seconds;
            speed = _speed;
            Time.captureFramerate = 0;
        }

        isStart = true;
    }

    public void ResetStartTime()
    {
        offset = 0f;
        isStart = false;
    }
}
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using MajSimai;

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

    public float GetCurrentBpm(List<SimaiTimingPoint> timings)
    {
    if (timings == null || timings.Count == 0) return -1f;
    var tp = timings.LastOrDefault(t => t.Timing <= AudioTime);
    return tp?.Bpm > 0f ? tp.Bpm : -1f;
    }


    public void SetStartTime(long _ticks, float _offset, float _speed, bool _isRecord = false)
    {
        ticks = _ticks;
        offset = _offset;
        AudioTime = offset;
        var dateTime = new DateTime(ticks);
        var seconds = (dateTime - DateTime.Now).TotalSeconds;
        isRecord = _isRecord;

        var loader = GameObject.Find("DataLoader").GetComponent<JsonDataLoader>();
        float bpm = GetCurrentBpm(loader.Timings);
        if (_isRecord)
        {
            if (bpm > 0f)
            {
                float secondsPerBeat = 60f / bpm;
                startTime = Time.time + 5f + 4f * secondsPerBeat;
            }
            else
            {
                startTime = Time.time + 5f;
            }

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
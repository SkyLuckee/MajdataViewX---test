using System;
using UnityEngine;
using UnityEngine.UI;

public class AudioTimeProvider : MonoBehaviour
{
    public float AudioTime; //notes get this value
    public bool isStart;
    public bool isRecord;
    public float offset;
    private float speed;

    private float startTime;
    private long ticks;
    public float InitialBpm { get; private set; } = 120f;
    public void SetInitialBpm(float bpm)
    {
        InitialBpm = bpm;
        if (isStart && useClockCount)
        {
            if (isRecord)
            {
                if (Time.time < startTime)
                startTime = Time.time + 5f + 240f / InitialBpm;
            }
            else
            {
                if (Time.realtimeSinceStartup < startTime)
                startTime = Time.realtimeSinceStartup + 5f + 240f / InitialBpm;
            }
        }
    }
    private bool useClockCount;

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

    public void SetStartTime(long _ticks, float _offset, float _speed, bool _isRecord = false, float bpm = -1f, bool _useClockCount = false)
    {
        ticks = _ticks;
        offset = _offset;
        AudioTime = offset;
        if (bpm > 0f)
            InitialBpm = bpm;
        var dateTime = new DateTime(ticks);
        var seconds = (dateTime - DateTime.Now).TotalSeconds;
        isRecord = _isRecord;
        useClockCount = isRecord || _useClockCount;
        if (useClockCount)
        {
            if (isRecord)
            {
                startTime = Time.time + 5f + 240f / InitialBpm;
                Time.timeScale = _speed;
                Time.captureFramerate = 60;
            }
            else
            {
                startTime = Time.realtimeSinceStartup + 5f + 240f / InitialBpm;
                speed = _speed;
                Time.captureFramerate = 0;
            }
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
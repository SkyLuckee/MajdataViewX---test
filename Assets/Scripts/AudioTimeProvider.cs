using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using MajSimai;
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
    public float CurrentBpm { get; private set; }

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
            // Update BPM live
            var loader = GameObject.Find("DataLoader").GetComponent<JsonDataLoader>();
            CurrentBpm = GetCurrentBpm(loader.Timings);
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

        // If AudioTime is 0 or before the first timing point, use the first BPM
        if (AudioTime <= timings[0].Timing)
            return timings[0].Bpm;

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
                startTime = Time.time + 4f * secondsPerBeat;
                GameObject.Find("ErrText").GetComponent<Text>().text = $"Current BPM: {bpm}";
            }
            else
            {
                startTime = Time.time + 5f;
                GameObject.Find("ErrText").GetComponent<Text>().text = "Current BPM not found";
            }

            Time.timeScale = _speed;
            Time.captureFramerate = 60;
        }
        else
        {
            startTime = Time.realtimeSinceStartup + (float)seconds;
            speed = _speed;
            Time.captureFramerate = 0;
            GameObject.Find("ErrText").GetComponent<Text>().text = $"Current BPM (not recorded): {bpm}";
        }

        isStart = true;
    }

    public void ResetStartTime()
    {
        offset = 0f;
        isStart = false;
    }
}
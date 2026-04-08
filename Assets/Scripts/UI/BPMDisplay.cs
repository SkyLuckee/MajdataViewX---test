using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using MajSimai;

public class BpmDisplay : MonoBehaviour
{
    [Header("References")]
    public JsonDataLoader jsonLoader;
    public AudioTimeProvider audioTimeProvider;

    [Header("UI")]
    public TextMeshProUGUI bpmTextTMP;
    public UnityEngine.UI.Text bpmTextUI;

    [Header("Options")]
    public string prefix = "BPM: ";
    public bool showDecimal = false;

    // timeline of BPM change events (time in seconds, bpm)
    private List<(double time, float bpm)> bpmEvents = new();
    private int bpmIndex = 0;
    private float displayedBpm = -1f;
    private bool initialized = false;

    // Add to BpmDisplay
    public void PrintBpmTimeline()
    {
        if (bpmEvents == null || bpmEvents.Count == 0)
        {
            Debug.Log("BpmDisplay: no BPM events available.");
            return;
        }

        Debug.Log($"BpmDisplay: printing {bpmEvents.Count} BPM events");
        for (int i = 0; i < bpmEvents.Count; i++)
        {
            var ev = bpmEvents[i];
            Debug.LogFormat("BPM Event {0}: time = {1:F3}s, bpm = {2:F2}", i, ev.time, ev.bpm);
        }

        // Print a few sample lookups around current audio time
        if (audioTimeProvider != null)
        {
            double now = audioTimeProvider.AudioTime;
            Debug.LogFormat("BpmDisplay: current audio time = {0:F3}s", now);

            // find index for now
            int idx = bpmIndex;
            while (idx + 1 < bpmEvents.Count && bpmEvents[idx + 1].time <= now) idx++;
            while (idx > 0 && bpmEvents[idx].time > now) idx--;

            Debug.LogFormat("Nearest BPM event index = {0}, event time = {1:F3}s, bpm = {2:F2}", idx, bpmEvents[idx].time, bpmEvents[idx].bpm);

            // print next few events if any
            for (int j = idx; j < Mathf.Min(idx + 4, bpmEvents.Count); j++)
            {
                var e = bpmEvents[j];
                Debug.LogFormat("  Next {0}: time = {1:F3}s, bpm = {2:F2}", j - idx, e.time, e.bpm);
            }
        }
    }

    private void Start()
    {
        TryInitialize();
        UpdateBpmText(displayedBpm);
    }

    private void Update()
    {
        // try to initialize if loader finished after Start
        if (!initialized)
        {
            TryInitialize();
            if (!initialized) return;
        }

        if (audioTimeProvider == null) return;

        double audioTime = audioTimeProvider.AudioTime;

        // advance bpmIndex while next event time <= audioTime
        while (bpmIndex + 1 < bpmEvents.Count && bpmEvents[bpmIndex + 1].time <= audioTime)
        {
            bpmIndex++;
        }

        // determine effective bpm
        float bpm = -1f;
        if (bpmEvents.Count > 0)
        {
            // if audioTime is before first event, use first event bpm
            if (audioTime < bpmEvents[0].time)
                bpm = bpmEvents[0].bpm;
            else
                bpm = bpmEvents[Mathf.Clamp(bpmIndex, 0, bpmEvents.Count - 1)].bpm;
        }
        else if (jsonLoader != null)
        {
            // fallback to loader's currentBpm if no events found
            bpm = jsonLoader.currentBpm;
        }

        if (!Mathf.Approximately(bpm, displayedBpm))
        {
            displayedBpm = bpm;
            UpdateBpmText(displayedBpm);
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            PrintBpmTimeline();
        }
    }

    private void TryInitialize()
    {
        var list = jsonLoader?.GetTimingList();
        if (list == null) return;

        var timings = list.ToList();
        if (timings.Count == 0) return;

        // sort by timing ascending
        timings.Sort((a, b) => a.Timing.CompareTo(b.Timing));

        // build bpmEvents from timing points that have a valid BPM (>= 0)
        bpmEvents.Clear();
        foreach (var t in timings)
        {
            if (t.Bpm >= 0f)
            {
                bpmEvents.Add((t.Timing, t.Bpm));
            }
        }

        // If no explicit BPM events found, try to use jsonLoader.currentBpm as a single event at time 0
        if (bpmEvents.Count == 0 && jsonLoader != null && jsonLoader.currentBpm > 0f)
        {
            bpmEvents.Add((0.0, jsonLoader.currentBpm));
        }

        // If there are BPM events, ensure the first event covers time 0 (so we always have a value)
        if (bpmEvents.Count > 0 && bpmEvents[0].time > 0.0)
        {
            // duplicate first BPM at time 0
            var first = bpmEvents[0];
            bpmEvents.Insert(0, (0.0, first.bpm));
        }

        // final safety: if still empty, leave uninitialized and rely on jsonLoader.currentBpm later
        if (bpmEvents.Count > 0)
        {
            initialized = true;
            bpmIndex = 0;
            displayedBpm = bpmEvents[0].bpm;
            Debug.Log($"BpmDisplay initialized: {bpmEvents.Count} BPM events, first BPM={displayedBpm}");
        }
    }

    private void UpdateBpmText(float bpm)
    {
        if (bpm <= 0f)
        {
            // placeholder when unknown
            if (bpmTextTMP != null) bpmTextTMP.text = $"{prefix}—";
            if (bpmTextUI != null) bpmTextUI.text = $"{prefix}—";
            return;
        }

        string text = showDecimal ? $"{prefix}{bpm:F2}" : $"{prefix}{Mathf.RoundToInt(bpm)}";
        if (bpmTextTMP != null) bpmTextTMP.text = text;
        if (bpmTextUI != null) bpmTextUI.text = text;
    }
}

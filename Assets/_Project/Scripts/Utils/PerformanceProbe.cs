using UnityEngine;

namespace Shrine.Utils
{
    /// <summary>
    /// Lightweight frame probe used for the PERFORMANCE.md measurements. It samples
    /// a fixed window after boot and logs one summary line; it has no UI and is
    /// compiled out of nothing, so it can stay enabled in release builds.
    /// </summary>
    public sealed class PerformanceProbe : MonoBehaviour
    {
        private const int Warmup = 120;
        private const int Window = 600;

        private double _totalMs;
        private long _startMemory;
        private long _endMemory;
        private int _frames;
        private int _samples;
        private bool _done;

        private void Awake()
        {
            _startMemory = System.GC.GetTotalMemory(false);
        }

        private void Update()
        {
            if (_done) return;
            if (_frames++ < Warmup)
            {
                if (_frames == Warmup) _startMemory = System.GC.GetTotalMemory(false);
                return;
            }
            _totalMs += Time.unscaledDeltaTime * 1000f;
            _samples++;
            if (_samples < Window) return;
            _done = true;
            _endMemory = System.GC.GetTotalMemory(false);
            float average = (float)(_totalMs / _samples);
            float allocatedPerFrame = (_endMemory - _startMemory) / (float)_samples;
            Debug.Log($"[ShrinePerf] frames={_samples} avg={average:F2}ms " +
                $"estimated_alloc={allocatedPerFrame:F0}B/frame");
        }
    }
}

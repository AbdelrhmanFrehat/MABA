using System.Windows.Threading;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class CncPreviewPlaybackService : ICncPreviewPlaybackService
{
    private const decimal DefaultRapidMmPerMinute = 6000m;
    private const decimal DefaultCutMmPerMinute = 1200m;
    private readonly List<GcodeMotionCommand> _loadedMotions = new();
    private readonly List<GcodeMotionCommand> _activePlaybackMotions = new();
    private readonly DispatcherTimer _timer;
    private CncPreviewSummary _summary = new();
    private CncPreviewSimulationState _simulationState = CncPreviewSimulationState.Idle;
    private int _currentSegmentIndex = -1;
    private double _segmentProgress;
    private double _playbackSpeed = 1d;
    private decimal _toolX;
    private decimal _toolY;
    private decimal _toolZ;
    private double _segmentElapsedMs;
    private DateTime _lastTick = DateTime.UtcNow;
    private bool _isFramePlayback;

    public CncPreviewPlaybackService()
    {
        _timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(33)
        };
        _timer.Tick += OnTick;
    }

    public IReadOnlyList<GcodeMotionCommand> LoadedMotions => _loadedMotions;
    public IReadOnlyList<GcodeMotionCommand> ActivePlaybackMotions => _activePlaybackMotions;
    public CncPreviewSimulationState SimulationState => _simulationState;
    public int CurrentSegmentIndex => _currentSegmentIndex;
    public double SegmentProgress => _segmentProgress;
    public decimal ToolX => _toolX;
    public decimal ToolY => _toolY;
    public decimal ToolZ => _toolZ;
    public double PlaybackSpeed => _playbackSpeed;
    public CncPreviewSummary Summary => _summary;
    public bool IsFramePlayback => _isFramePlayback;
    public event EventHandler? PlaybackChanged;

    public void Load(IReadOnlyList<GcodeMotionCommand> motions)
    {
        _loadedMotions.Clear();
        _loadedMotions.AddRange(motions.Where(m => m.IsExecutable));
        _activePlaybackMotions.Clear();
        _activePlaybackMotions.AddRange(_loadedMotions);
        _isFramePlayback = false;
        RecalculateSummary(_activePlaybackMotions);
        ResetPlaybackState();
        if (_activePlaybackMotions.Count > 0)
        {
            _toolX = _activePlaybackMotions[0].StartX;
            _toolY = _activePlaybackMotions[0].StartY;
            _toolZ = _activePlaybackMotions[0].StartZ;
        }

        NotifyChanged();
    }

    public void Play()
    {
        if (_loadedMotions.Count == 0)
            throw new InvalidOperationException("Load a G-code file before starting preview playback.");

        _activePlaybackMotions.Clear();
        _activePlaybackMotions.AddRange(_loadedMotions);
        _isFramePlayback = false;
        RecalculateSummary(_activePlaybackMotions);

        if (_simulationState is CncPreviewSimulationState.Completed or CncPreviewSimulationState.Ready)
            ResetPlaybackState();

        if (_currentSegmentIndex < 0)
            _currentSegmentIndex = 0;

        _simulationState = CncPreviewSimulationState.Playing;
        _lastTick = DateTime.UtcNow;
        _timer.Start();
        NotifyChanged();
    }

    public void PlayFrame(IReadOnlyList<GcodeMotionCommand> frameMotions)
    {
        _activePlaybackMotions.Clear();
        _activePlaybackMotions.AddRange(frameMotions.Where(m => m.IsExecutable));
        if (_activePlaybackMotions.Count == 0)
            throw new InvalidOperationException("Load a valid G-code job before starting frame preview.");

        _isFramePlayback = true;
        RecalculateSummary(_activePlaybackMotions);
        ResetPlaybackState();
        _currentSegmentIndex = 0;
        _simulationState = CncPreviewSimulationState.Playing;
        _lastTick = DateTime.UtcNow;
        _timer.Start();
        NotifyChanged();
    }

    public void Pause()
    {
        if (_simulationState != CncPreviewSimulationState.Playing)
            return;

        _timer.Stop();
        _simulationState = CncPreviewSimulationState.Paused;
        NotifyChanged();
    }

    public void Stop()
    {
        _timer.Stop();
        if (_isFramePlayback)
        {
            _activePlaybackMotions.Clear();
            _activePlaybackMotions.AddRange(_loadedMotions);
            _isFramePlayback = false;
            RecalculateSummary(_activePlaybackMotions);
        }

        ResetPlaybackState();
        NotifyChanged();
    }

    public void SetPlaybackSpeed(double speed)
    {
        _playbackSpeed = Math.Clamp(speed, 0.5d, 4d);
        NotifyChanged();
    }

    private void OnTick(object? sender, EventArgs e)
    {
        if (_simulationState != CncPreviewSimulationState.Playing || _currentSegmentIndex < 0 || _currentSegmentIndex >= _activePlaybackMotions.Count)
            return;

        var now = DateTime.UtcNow;
        var deltaMs = (now - _lastTick).TotalMilliseconds;
        _lastTick = now;
        _segmentElapsedMs += deltaMs * _playbackSpeed;

        while (_currentSegmentIndex >= 0 && _currentSegmentIndex < _activePlaybackMotions.Count)
        {
            var motion = _activePlaybackMotions[_currentSegmentIndex];
            var durationMs = GetSegmentDurationMilliseconds(motion);
            var progress = durationMs <= 0.001 ? 1d : Math.Clamp(_segmentElapsedMs / durationMs, 0d, 1d);
            _segmentProgress = progress;
            _toolX = Interpolate(motion.StartX, motion.EndX, progress);
            _toolY = Interpolate(motion.StartY, motion.EndY, progress);
            _toolZ = Interpolate(motion.StartZ, motion.EndZ, progress);

            if (progress < 1d)
                break;

            _currentSegmentIndex++;
            _segmentElapsedMs = 0;
            _segmentProgress = 0;

            if (_currentSegmentIndex >= _activePlaybackMotions.Count)
            {
                _currentSegmentIndex = _activePlaybackMotions.Count - 1;
                _segmentProgress = 1d;
                _toolX = motion.EndX;
                _toolY = motion.EndY;
                _toolZ = motion.EndZ;
                _simulationState = CncPreviewSimulationState.Completed;
                _timer.Stop();
                break;
            }
        }

        NotifyChanged();
    }

    private void ResetPlaybackState()
    {
        _timer.Stop();
        _simulationState = _activePlaybackMotions.Count > 0
            ? CncPreviewSimulationState.Ready
            : CncPreviewSimulationState.Idle;
        _currentSegmentIndex = -1;
        _segmentProgress = 0;
        _segmentElapsedMs = 0;

        if (_activePlaybackMotions.Count > 0)
        {
            _toolX = _activePlaybackMotions[0].StartX;
            _toolY = _activePlaybackMotions[0].StartY;
            _toolZ = _activePlaybackMotions[0].StartZ;
        }
        else
        {
            _toolX = 0;
            _toolY = 0;
            _toolZ = 0;
        }
    }

    private void RecalculateSummary(IEnumerable<GcodeMotionCommand> motions)
    {
        _summary = new CncPreviewSummary();

        foreach (var motion in motions)
        {
            var length = motion.LengthMm;
            _summary.TotalDistanceMm += length;
            if (motion.IsRapidMove)
                _summary.RapidDistanceMm += length;
            else
                _summary.CutDistanceMm += length;

            var mmPerMinute = motion.FeedRate.HasValue && motion.FeedRate.Value > 0m
                ? motion.FeedRate.Value
                : motion.IsRapidMove
                    ? DefaultRapidMmPerMinute
                    : DefaultCutMmPerMinute;

            var seconds = mmPerMinute <= 0m ? 0d : (double)(length / mmPerMinute) * 60d;
            _summary.EstimatedTime += TimeSpan.FromSeconds(seconds);
        }
    }

    private static double GetSegmentDurationMilliseconds(GcodeMotionCommand motion)
    {
        var mmPerMinute = motion.FeedRate.HasValue && motion.FeedRate.Value > 0m
            ? motion.FeedRate.Value
            : motion.IsRapidMove
                ? DefaultRapidMmPerMinute
                : DefaultCutMmPerMinute;

        if (mmPerMinute <= 0m)
            return 120d;

        var seconds = (double)(motion.LengthMm / mmPerMinute) * 60d;
        return Math.Max(120d, seconds * 1000d);
    }

    private static decimal Interpolate(decimal start, decimal end, double progress)
    {
        return start + ((end - start) * (decimal)progress);
    }

    private void NotifyChanged()
    {
        PlaybackChanged?.Invoke(this, EventArgs.Empty);
    }
}

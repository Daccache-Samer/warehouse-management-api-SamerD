using System.Collections.Concurrent;

namespace Warehouse.Application.Trackers;

public class CacheStatisticsTracker
{
    private int _hitCount;
    private int _missCount;
    private readonly ConcurrentDictionary<string, byte> _cachedKeys = new();
    private DateTime? _lastRefreshTime;

    public int HitCount => _hitCount;
    public int MissCount => _missCount;
    public IReadOnlyCollection<string> CachedKeys => _cachedKeys.Keys.ToList().AsReadOnly();
    public DateTime? LastRefreshTime => _lastRefreshTime;

    public void RecordHit() => Interlocked.Increment(ref _hitCount);

    public void RecordMiss(string key)
    {
        Interlocked.Increment(ref _missCount);
        _cachedKeys.TryAdd(key, 0);
        _lastRefreshTime = DateTime.UtcNow;
    }
    
    // Optional: Call this from write handlers when you remove a key
    public void RemoveKey(string key) => _cachedKeys.TryRemove(key, out _);
}
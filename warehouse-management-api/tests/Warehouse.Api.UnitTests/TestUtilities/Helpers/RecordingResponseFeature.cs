using Microsoft.AspNetCore.Http.Features;

namespace Warehouse.Api.UnitTests.TestUtilities.Helpers;

public sealed class RecordingResponseFeature : HttpResponseFeature
{
    private readonly List<(Func<object, Task> Callback, object State)> _onStartingCallbacks = new();

    public override void OnStarting(Func<object, Task> callback, object state)
        => _onStartingCallbacks.Add((callback, state));

    public async Task FireOnStartingAsync()
    {
        foreach (var (callback, state) in _onStartingCallbacks)
        {
            await callback(state);
        }
    }
}
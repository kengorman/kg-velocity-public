using Microsoft.JSInterop;

namespace Kg.Velocity.Blazor.Services;

public class GameLoop : IDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private DotNetObjectReference<GameLoop>? _dotNetRef;
    private bool _isRunning;

    public event Action? OnUpdate;

    public GameLoop(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task Start()
    {
        if (_isRunning) return;

        _dotNetRef = DotNetObjectReference.Create(this);
        await _jsRuntime.InvokeVoidAsync("gameLoop.start", _dotNetRef);
        _isRunning = true;
    }

    public async Task Stop()
    {
        if (!_isRunning) return;

        await _jsRuntime.InvokeVoidAsync("gameLoop.stop");
        _dotNetRef?.Dispose();
        _dotNetRef = null;
        _isRunning = false;
    }

    [JSInvokable]
    public void OnFrame()
    {
        OnUpdate?.Invoke();
    }

    public void Dispose()
    {
        if (_isRunning)
        {
            // Fire and forget stop
            _ = Stop();
        }
    }
}


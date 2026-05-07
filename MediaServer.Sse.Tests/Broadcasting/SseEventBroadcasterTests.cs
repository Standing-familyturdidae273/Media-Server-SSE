using MediaServer.Sse.Core.Broadcasting;
using MediaServer.Sse.Core.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MediaServer.Sse.Tests.Broadcasting;

public class SseEventBroadcasterTests : IDisposable
{
    private readonly SseEventBroadcaster _broadcaster;

    public SseEventBroadcasterTests()
    {
        _broadcaster = new SseEventBroadcaster(NullLogger<SseEventBroadcaster>.Instance);
    }

    public void Dispose()
    {
        _broadcaster.Dispose();
    }

    [Fact]
    public void Subscribe_ReturnsUniqueIdAndReader()
    {
        var (id1, reader1) = _broadcaster.Subscribe();
        var (id2, reader2) = _broadcaster.Subscribe();

        Assert.NotEqual(id1, id2);
        Assert.NotNull(reader1);
        Assert.NotNull(reader2);
    }

    [Fact]
    public void Broadcast_DeliversEventToAllSubscribers()
    {
        var (_, reader1) = _broadcaster.Subscribe();
        var (_, reader2) = _broadcaster.Subscribe();

        var evt = new SseEvent { EventType = "playing", SessionId = "s1", State = "playing" };
        _broadcaster.Broadcast(evt);

        Assert.True(reader1.TryRead(out var received1));
        Assert.Equal("s1", received1!.SessionId);

        Assert.True(reader2.TryRead(out var received2));
        Assert.Equal("s1", received2!.SessionId);
    }

    [Fact]
    public void Broadcast_DoesNotDeliverToUnsubscribed()
    {
        var (id1, reader1) = _broadcaster.Subscribe();
        var (_, reader2) = _broadcaster.Subscribe();

        _broadcaster.Unsubscribe(id1);

        var evt = new SseEvent { EventType = "playing", SessionId = "s1" };
        _broadcaster.Broadcast(evt);

        Assert.False(reader1.TryRead(out _));
        Assert.True(reader2.TryRead(out _));
    }

    [Fact]
    public void Broadcast_DropsEventForFullChannel()
    {
        var (_, reader) = _broadcaster.Subscribe();

        for (int i = 0; i < 100; i++)
        {
            _broadcaster.Broadcast(new SseEvent { EventType = "progress", SessionId = $"s{i}" });
        }

        // DropWrite silently drops the newest item when full — TryWrite still returns true
        _broadcaster.Broadcast(new SseEvent { EventType = "progress", SessionId = "overflow" });

        int count = 0;
        while (reader.TryRead(out _))
        {
            count++;
        }

        Assert.Equal(100, count);
    }

    [Fact]
    public async Task PingTimer_SendsPingEvents()
    {
        using var fastBroadcaster = new SseEventBroadcaster(
            NullLogger<SseEventBroadcaster>.Instance,
            pingIntervalMs: 100);

        var (_, reader) = fastBroadcaster.Subscribe();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var evt = await reader.ReadAsync(cts.Token);

        Assert.Equal("ping", evt.EventType);
    }
}

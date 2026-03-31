using System;
using System.Collections.Generic;
using System.Threading.Channels;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Logging;
using MediaServer.Sse.Core.Broadcasting;
using MediaServer.Sse.Core.Models;
using Moq;
using Xunit;

namespace Emby.Plugin.Sse.Tests;

public class SseEntryPointTests : IDisposable
{
    private readonly Mock<ISessionManager> _sessionManager;
    private readonly Mock<ILogManager> _logManager;
    private readonly Mock<ILogger> _logger;

    public SseEntryPointTests()
    {
        _sessionManager = new Mock<ISessionManager>();
        _logManager = new Mock<ILogManager>();
        _logger = new Mock<ILogger>();
        _logManager.Setup(m => m.GetLogger(It.IsAny<string>())).Returns(_logger.Object);
    }

    public void Dispose()
    {
        // Clean up static state between tests
        SseEntryPoint.Broadcaster?.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Run_SetsBroadcasterStaticProperty()
    {
        var entryPoint = new SseEntryPoint(_sessionManager.Object, _logManager.Object);

        entryPoint.Run();

        Assert.NotNull(SseEntryPoint.Broadcaster);
        entryPoint.Dispose();
    }

    [Fact]
    public void Dispose_ClearsBroadcaster()
    {
        var entryPoint = new SseEntryPoint(_sessionManager.Object, _logManager.Object);
        entryPoint.Run();
        Assert.NotNull(SseEntryPoint.Broadcaster);

        entryPoint.Dispose();

        Assert.Null(SseEntryPoint.Broadcaster);
    }

    [Fact]
    public void PlaybackStart_BroadcastsPlayingEvent()
    {
        var entryPoint = new SseEntryPoint(_sessionManager.Object, _logManager.Object);
        entryPoint.Run();

        var userId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var args = CreatePlaybackProgressArgs(userId, itemId, sessionId: "session1", isPaused: false, positionTicks: 100);
        var reader = Subscribe();

        _sessionManager.Raise(m => m.PlaybackStart += null, _sessionManager.Object, args);

        var evt = ReadSingle(reader);
        Assert.NotNull(evt);
        Assert.Equal("playing", evt!.EventType);
        Assert.Equal("session1", evt.SessionId);
        Assert.Equal(itemId.ToString("N"), evt.ItemId);
        Assert.Equal(userId.ToString("N"), evt.UserId);
        Assert.Equal("playing", evt.State);

        entryPoint.Dispose();
    }

    [Fact]
    public void PlaybackProgress_NotPaused_BroadcastsProgressEvent()
    {
        var entryPoint = new SseEntryPoint(_sessionManager.Object, _logManager.Object);
        entryPoint.Run();

        var userId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var args = CreatePlaybackProgressArgs(userId, itemId, sessionId: "s1", isPaused: false, positionTicks: 10000);
        var reader = Subscribe();

        _sessionManager.Raise(m => m.PlaybackProgress += null, _sessionManager.Object, args);

        var evt = ReadSingle(reader);
        Assert.NotNull(evt);
        Assert.Equal("progress", evt!.EventType);
        Assert.Equal("playing", evt.State);
        Assert.Equal(10000L, evt.PositionTicks);

        entryPoint.Dispose();
    }

    [Fact]
    public void PlaybackProgress_Paused_BroadcastsPausedEvent()
    {
        var entryPoint = new SseEntryPoint(_sessionManager.Object, _logManager.Object);
        entryPoint.Run();

        var userId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var args = CreatePlaybackProgressArgs(userId, itemId, sessionId: "s1", isPaused: true, positionTicks: 5000);
        var reader = Subscribe();

        _sessionManager.Raise(m => m.PlaybackProgress += null, _sessionManager.Object, args);

        var evt = ReadSingle(reader);
        Assert.NotNull(evt);
        Assert.Equal("paused", evt!.EventType);
        Assert.Equal("paused", evt.State);
        Assert.Equal(5000L, evt.PositionTicks);

        entryPoint.Dispose();
    }

    [Fact]
    public void PlaybackStopped_BroadcastsStoppedEventWithPlayedToCompletion()
    {
        var entryPoint = new SseEntryPoint(_sessionManager.Object, _logManager.Object);
        entryPoint.Run();

        var userId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var args = CreatePlaybackStopArgs(userId, itemId, sessionId: "session1", playedToCompletion: true);
        var reader = Subscribe();

        _sessionManager.Raise(m => m.PlaybackStopped += null, _sessionManager.Object, args);

        var evt = ReadSingle(reader);
        Assert.NotNull(evt);
        Assert.Equal("stopped", evt!.EventType);
        Assert.Equal("stopped", evt.State);
        Assert.True(evt.PlayedToCompletion);

        entryPoint.Dispose();
    }

    [Fact]
    public void SessionStarted_BroadcastsSessionStartEvent()
    {
        var entryPoint = new SseEntryPoint(_sessionManager.Object, _logManager.Object);
        entryPoint.Run();

        var args = new SessionEventArgs
        {
            SessionInfo = new SessionInfo { Id = "sess-42", UserId = "user-abc" }
        };
        var reader = Subscribe();

        _sessionManager.Raise(m => m.SessionStarted += null, _sessionManager.Object, args);

        var evt = ReadSingle(reader);
        Assert.NotNull(evt);
        Assert.Equal("session.start", evt!.EventType);
        Assert.Equal("sess-42", evt.SessionId);
        Assert.Equal("user-abc", evt.UserId);

        entryPoint.Dispose();
    }

    [Fact]
    public void SessionEnded_BroadcastsSessionEndEvent()
    {
        var entryPoint = new SseEntryPoint(_sessionManager.Object, _logManager.Object);
        entryPoint.Run();

        var args = new SessionEventArgs
        {
            SessionInfo = new SessionInfo { Id = "sess-99", UserId = "user-xyz" }
        };
        var reader = Subscribe();

        _sessionManager.Raise(m => m.SessionEnded += null, _sessionManager.Object, args);

        var evt = ReadSingle(reader);
        Assert.NotNull(evt);
        Assert.Equal("session.end", evt!.EventType);
        Assert.Equal("sess-99", evt.SessionId);
        Assert.Equal("user-xyz", evt.UserId);

        entryPoint.Dispose();
    }

    [Fact]
    public void PlaybackStart_SkipsWhenNoUsers()
    {
        var entryPoint = new SseEntryPoint(_sessionManager.Object, _logManager.Object);
        entryPoint.Run();

        var args = CreatePlaybackProgressArgs(Guid.Empty, Guid.NewGuid(), "s1", false, 0, includeUser: false);
        var reader = Subscribe();

        _sessionManager.Raise(m => m.PlaybackStart += null, _sessionManager.Object, args);

        var evt = ReadSingle(reader);
        Assert.Null(evt);

        entryPoint.Dispose();
    }

    [Fact]
    public void PlaybackStart_SkipsWhenItemIsNull()
    {
        var entryPoint = new SseEntryPoint(_sessionManager.Object, _logManager.Object);
        entryPoint.Run();

        var userId = Guid.NewGuid();
        var user = new User { Id = userId };
        var args = new PlaybackProgressEventArgs
        {
            Item = null!,
            Session = new SessionInfo { Id = "s1" },
            Users = new List<User> { user },
            PlaybackPositionTicks = 0
        };
        var reader = Subscribe();

        _sessionManager.Raise(m => m.PlaybackStart += null, _sessionManager.Object, args);

        var evt = ReadSingle(reader);
        Assert.Null(evt);

        entryPoint.Dispose();
    }

    /// <summary>
    /// Subscribe to the broadcaster and return the channel reader.
    /// </summary>
    private ChannelReader<SseEvent> Subscribe()
    {
        var broadcaster = SseEntryPoint.Broadcaster!;
        var (_, reader) = broadcaster.Subscribe();
        return reader;
    }

    /// <summary>
    /// Try to read a single event from the channel. Returns null if no event is available.
    /// Since Broadcast writes synchronously to the channel, TryRead will succeed
    /// immediately after the event is raised.
    /// </summary>
    private static SseEvent? ReadSingle(ChannelReader<SseEvent> reader)
    {
        return reader.TryRead(out var evt) ? evt : null;
    }

    private static PlaybackProgressEventArgs CreatePlaybackProgressArgs(
        Guid userId, Guid itemId, string sessionId, bool isPaused, long positionTicks, bool includeUser = true)
    {
        var item = new Audio { Id = itemId };
        var session = new SessionInfo { Id = sessionId };

        var args = new PlaybackProgressEventArgs
        {
            Item = item,
            Session = session,
            PlaybackPositionTicks = positionTicks,
            IsPaused = isPaused
        };

        if (includeUser)
        {
            var user = new User { Id = userId };
            args.Users = new List<User> { user };
        }
        else
        {
            args.Users = new List<User>();
        }

        return args;
    }

    private static PlaybackStopEventArgs CreatePlaybackStopArgs(
        Guid userId, Guid itemId, string sessionId, bool playedToCompletion)
    {
        var item = new Audio { Id = itemId };
        var session = new SessionInfo { Id = sessionId };
        var user = new User { Id = userId };

        return new PlaybackStopEventArgs
        {
            Item = item,
            Session = session,
            Users = new List<User> { user },
            PlaybackPositionTicks = 0,
            PlayedToCompletion = playedToCompletion
        };
    }
}

using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HOB.Worker.Observers;

public class QueueDrainObserver : IReceiveObserver
{
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly ILogger<QueueDrainObserver> _logger;
    private Timer? _idleTimer;
    private int _activeMessages = 0;
    private readonly object _lock = new();
    private const int IdleTimeoutSeconds = 30;

    public QueueDrainObserver(IHostApplicationLifetime appLifetime, ILogger<QueueDrainObserver> logger)
    {
        _appLifetime = appLifetime;
        _logger = logger;

        // Initialize timer (but don't start it yet)
        _idleTimer = new Timer(
            callback: OnIdleTimeout,
            state: null,
            dueTime: TimeSpan.FromSeconds(IdleTimeoutSeconds),
            period: Timeout.InfiniteTimeSpan
        );

        _logger.LogInformation("Queue drain observer initialized with {Timeout}s timeout", IdleTimeoutSeconds);
    }

    public Task PreReceive(ReceiveContext context)
    {
        lock (_lock)
        {
            _activeMessages++;
            _logger.LogDebug("Message received, active messages: {ActiveMessages}, pausing idle timer", _activeMessages);

            // Pause the timer while processing
            _idleTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        }

        return Task.CompletedTask;
    }

    public Task PostReceive(ReceiveContext context)
    {
        // Message was received but before consumer processes it
        return Task.CompletedTask;
    }

    public Task PostConsume<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType) where T : class
    {
        lock (_lock)
        {
            _activeMessages--;
            _logger.LogDebug("Message consumed in {Duration}ms, active messages: {ActiveMessages}", duration.TotalMilliseconds, _activeMessages);

            if (_activeMessages == 0)
            {
                _logger.LogInformation("No active messages, starting {Timeout}s idle timer", IdleTimeoutSeconds);
                // Reset the timer when all messages are processed
                _idleTimer?.Change(TimeSpan.FromSeconds(IdleTimeoutSeconds), Timeout.InfiniteTimeSpan);
            }
        }

        return Task.CompletedTask;
    }

    public Task ConsumeFault<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType, Exception exception) where T : class
    {
        lock (_lock)
        {
            _activeMessages--;
            _logger.LogError(exception, "Message consumption failed in {Duration}ms, active messages: {ActiveMessages}", duration.TotalMilliseconds, _activeMessages);

            if (_activeMessages == 0)
            {
                _logger.LogInformation("No active messages after fault, starting {Timeout}s idle timer", IdleTimeoutSeconds);
                // Reset the timer even after a fault
                _idleTimer?.Change(TimeSpan.FromSeconds(IdleTimeoutSeconds), Timeout.InfiniteTimeSpan);
            }
        }

        return Task.CompletedTask;
    }

    public Task ReceiveFault(ReceiveContext context, Exception exception)
    {
        _logger.LogError(exception, "Receive fault occurred");
        return Task.CompletedTask;
    }

    private void OnIdleTimeout(object? state)
    {
        lock (_lock)
        {
            if (_activeMessages == 0)
            {
                _logger.LogInformation("No messages received for {Timeout} seconds, initiating graceful shutdown", IdleTimeoutSeconds);
                _appLifetime.StopApplication();
            }
            else
            {
                _logger.LogDebug("Idle timeout occurred but {ActiveMessages} messages still active", _activeMessages);
            }
        }
    }
}

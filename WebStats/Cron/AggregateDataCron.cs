using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using WebStats.Persistence;

/// <summary>
/// Simple "cronjob" task, responsible for executing the aggregation task.
/// Runs between 02:00 and 03:00 in the morning, may drift a bit over time but will remain within this time period.
/// </summary>
public class AggregateDataCron : BackgroundService {
    private DateTimeOffset _nextRun = DateTimeOffset.UtcNow;
    private readonly Aggregator _aggregator;

    public AggregateDataCron(Aggregator aggregator) {
        _aggregator = aggregator;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        do {
            if (DateTimeOffset.UtcNow > _nextRun) {
                Process();
                _nextRun = DateTimeOffset.UtcNow.AddDays(1);

                if (_nextRun.Hour != 2)
                    _nextRun = _nextRun.AddHours(2 - _nextRun.Hour); // Runs 02:xx
            }

            await Task.Delay(1000 * 60 * 5, stoppingToken); // ~Every 5 minutes
        }
        while (!stoppingToken.IsCancellationRequested);
    }

    private void Process() {
        var serviceIdentifiers = _aggregator.ListServiceIdentifiers();
        var existing = _aggregator.GetRecentAggregates();
        foreach (var serviceId in serviceIdentifiers) {

            for (int i = 1; i <= 7; i++) {
                var d = RemoveTime(DateTimeOffset.UtcNow.AddDays(-i));
                if (!existing.Any(e => e.ServiceId == serviceId && StupidDateEquals(e.Date, d))) {
                    _aggregator.CreateAggregateEntry(serviceId, d);
                }
            }
        }

        _aggregator.Cleanup();
    }

    private bool StupidDateEquals(DateTimeOffset a, DateTimeOffset b) {
        return a.Year == b.Year && a.Month == b.Month && a.Day == b.Day;
    }

    // Surely this must exist from before?
    private DateTimeOffset RemoveTime(DateTimeOffset d) {
        if (d.Hour != 0)
            d = d.AddHours(-d.Hour);
        if (d.Minute != 0)
            d = d.AddMinutes(-d.Minute);
        if (d.Second != 0)
            d = d.AddSeconds(-d.Second);
        if (d.Millisecond != 0)
            d = d.AddMilliseconds(-d.Millisecond);

        return d;
    }
}